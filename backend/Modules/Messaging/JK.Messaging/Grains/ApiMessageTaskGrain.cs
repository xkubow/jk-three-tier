using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JK.Messaging.Contracts;
using JK.Messaging.Contracts.Enums;
using JK.Messaging.Database.Entities;
using JK.Messaging.Database.Repositories;
using JK.Messaging.Database;
using JK.Messaging.Models;
using JK.Messaging.States;
using JK.Platform.Core.Correlation;
using JK.Platform.Grpc.Client.Factory;
using JK.Platform.Persistence.EfCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JK.Messaging.Grains;

public sealed class ApiMessageTaskGrain : Grain, IRemindable, IApiMessageTaskGrain
{
    private readonly IPersistentState<ApiMessageTaskState> _taskState;
    private readonly ILogger<ApiMessageTaskGrain> _logger;
    private readonly IUnitOfWork<MessagingDbContext> _unitOfWork;
    private readonly IGrpcGenericClientFactory _genericClientFactory;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;
    private readonly IConfiguration _configuration;

    public ApiMessageTaskGrain(
        [PersistentState("ApiMessageTask", "orleans")]
        IPersistentState<ApiMessageTaskState> taskState,
        ILogger<ApiMessageTaskGrain> logger,
        IUnitOfWorkFactory<MessagingDbContext> unitOfWorkFactory,
        IGrpcGenericClientFactory genericClientFactory,
        ICorrelationContextAccessor correlationContextAccessor,
        IConfiguration configuration)
    {
        _taskState = taskState;
        _logger = logger;
        _unitOfWork = unitOfWorkFactory.Create();
        _genericClientFactory = genericClientFactory;
        _correlationContextAccessor = correlationContextAccessor;
        _configuration = configuration;
    }

    public async Task<bool> Register(RegisterApiMessageTaskCommand taskModel)
    {
        if (_taskState.State.TaskState != ApiMessageStateEnum.Waiting)
        {
            _logger.LogDebug("Task {TaskId} has already been processed or registered.", taskModel.Id);
            return false;
        }



        var delay = taskModel.Delay ?? TimeSpan.FromSeconds(1);
        var retryDelay = taskModel.RetryDelay ?? TimeSpan.FromMinutes(3);

        _taskState.State.TaskId = taskModel.Id;
        _taskState.State.TaskName = taskModel.TaskName;
        _taskState.State.TaskState = ApiMessageStateEnum.Created;
        _taskState.State.Attempts = 0;
        _taskState.State.MaxAttempts = taskModel.MaxAttempts <= 0 ? 5 : taskModel.MaxAttempts;
        _taskState.State.LastError = null;
        _taskState.State.CreatedOn = DateTime.UtcNow;
        _taskState.State.StartTime = DateTime.UtcNow.Add(delay);
        _taskState.State.FinishTime = null;
        _taskState.State.NextRetryOn = null;
        _taskState.State.OriginalCorrelationId = CorrelationContextAccessor.NormalizeOrCreate(
            taskModel.OriginalCorrelationId ?? _correlationContextAccessor.CorrelationId);

        await _taskState.WriteStateAsync();
        await UpsertTaskEntityAsync();

        await this.RegisterOrUpdateReminder(taskModel.TaskName, delay, retryDelay);
        return true;
    }

    public Task<ApiMessageTaskState> GetState()
    {
        return Task.FromResult(_taskState.State);
    }

    public async Task CancelAsync()
    {
        await DeactivateGrainAsync(_taskState.State.TaskName);

        _taskState.State.TaskState = ApiMessageStateEnum.Cancelled;
        _taskState.State.FinishTime = DateTime.UtcNow;
        _taskState.State.NextRetryOn = null;

        await _taskState.WriteStateAsync();
        await UpsertTaskEntityAsync();
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        var entity = await GetOrCreateTaskEntityAsync();

        // Sync from DB if needed
        entity.Attempts = _taskState.State.Attempts;
        entity.State = _taskState.State.TaskState;
        entity.LastError = _taskState.State.LastError;
        entity.NextRetryOn = _taskState.State.NextRetryOn;
        entity.FinishOn = _taskState.State.FinishTime;
        entity.OriginalCorrelationId = _taskState.State.OriginalCorrelationId;

        if (IsFinishState(_taskState.State.TaskState))
        {
            if (_taskState.State.TaskState != ApiMessageStateEnum.Failed ||
                _taskState.State.Attempts >= _taskState.State.MaxAttempts)
            {
                entity.FinishOn = _taskState.State.FinishTime ?? DateTime.UtcNow;
                await SaveTaskEntityAsync(entity);
                await DeactivateGrainAsync(reminderName);
                return;
            }
        }

        try
        {
            var originalCorrelationId = EnsureOriginalCorrelationId(entity);
            using var correlationScope = _correlationContextAccessor.Push(originalCorrelationId);

            _taskState.State.LastError = null;
            _taskState.State.NextRetryOn = null;
            entity.LastError = null;
            entity.NextRetryOn = null;

            await UpdateStateAsync(entity, ApiMessageStateEnum.Processing);

            var consumerUrls = GetConsumerUrlsFromConfiguration(_taskState.State.TaskName);
            var consumerResults = InitializeConsumerResults(consumerUrls, _taskState.State.ConsumerResults);
            var urlsToProcess = GetUrlsToProcess(consumerUrls, consumerResults);

            foreach (var url in urlsToProcess)
            {
                try
                {
                    await SendGrpcMessageAsync(url);
                    consumerResults[url] = "Success";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send message to consumer: {Url}", url);
                    consumerResults[url] = $"Error: {ex.Message}";
                }
            }

            _taskState.State.ConsumerResults = consumerResults;
            entity.ConsumerResults = System.Text.Json.JsonSerializer.Serialize(consumerResults);

            var failedUrls = consumerResults
                .Where(result => !IsSuccessResult(result.Value))
                .ToList();

            if (failedUrls.Count == 0)
            {
                _taskState.State.LastError = null;
                _taskState.State.NextRetryOn = null;

                await UpdateStateAsync(entity, ApiMessageStateEnum.Done);
                await DeactivateGrainAsync(reminderName);
                return;
            }

            _taskState.State.Attempts++;
            _taskState.State.LastError = BuildLastError(failedUrls);

            if (_taskState.State.Attempts >= _taskState.State.MaxAttempts)
            {
                _taskState.State.NextRetryOn = null;
                await UpdateStateAsync(entity, ApiMessageStateEnum.Failed);
                await DeactivateGrainAsync(reminderName);
                return;
            }

            _taskState.State.NextRetryOn = DateTime.UtcNow.AddMinutes(3);
            _taskState.State.TaskState = ApiMessageStateEnum.Failed;

            entity.Attempts = _taskState.State.Attempts;
            entity.State = _taskState.State.TaskState;
            entity.LastError = _taskState.State.LastError;
            entity.NextRetryOn = _taskState.State.NextRetryOn;
            entity.ConsumerResults = System.Text.Json.JsonSerializer.Serialize(consumerResults);

            await _taskState.WriteStateAsync();
            await SaveTaskEntityAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing API message task failed. TaskId: {TaskId}", _taskState.State.TaskId);

            _taskState.State.Attempts++;
            _taskState.State.LastError = ex.Message;

            if (_taskState.State.Attempts >= _taskState.State.MaxAttempts)
            {
                _taskState.State.NextRetryOn = null;
                await UpdateStateAsync(entity, ApiMessageStateEnum.Failed);
                await DeactivateGrainAsync(reminderName);
                return;
            }

            _taskState.State.NextRetryOn = DateTime.UtcNow.AddMinutes(3);
            _taskState.State.TaskState = ApiMessageStateEnum.Failed;

            entity.Attempts = _taskState.State.Attempts;
            entity.State = _taskState.State.TaskState;
            entity.LastError = _taskState.State.LastError;
            entity.NextRetryOn = _taskState.State.NextRetryOn;

            await _taskState.WriteStateAsync();
            await SaveTaskEntityAsync(entity);
        }
    }

    private List<string> GetConsumerUrlsFromConfiguration(string taskName)
    {
        var urls = _configuration.GetSection($"Messaging:Tasks:{taskName}:Urls").Get<List<string>>();

        if (urls == null || urls.Count == 0)
        {
            urls = _configuration.GetSection($"Messaging:RecurrentTasks:{taskName}:Urls").Get<List<string>>();
        }

        if (urls == null || urls.Count == 0)
        {
            urls = _configuration.GetSection($"{taskName}:Task:Consumers").Get<List<string>>();
        }

        if (urls == null || urls.Count == 0)
        {
            _logger.LogWarning("No consumer URLs found for task: {TaskName}", taskName);
            return new List<string>();
        }

        return urls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct()
            .ToList();
    }

    private static Dictionary<string, string> InitializeConsumerResults(
        IEnumerable<string> consumerUrls,
        Dictionary<string, string>? existingResults)
    {
        var consumerResults = new Dictionary<string, string>();

        foreach (var url in consumerUrls)
        {
            if (existingResults != null && existingResults.TryGetValue(url, out var existingResult))
            {
                consumerResults[url] = existingResult;
            }
        }

        return consumerResults;
    }

    private static List<string> GetUrlsToProcess(
        IEnumerable<string> consumerUrls,
        IReadOnlyDictionary<string, string> consumerResults)
    {
        return consumerUrls
            .Where(url => !consumerResults.TryGetValue(url, out var result) || !IsSuccessResult(result))
            .ToList();
    }

    private static bool IsSuccessResult(string? result)
        => string.Equals(result, "Success", StringComparison.Ordinal);

    private static string BuildLastError(IEnumerable<KeyValuePair<string, string>> failedUrls)
    {
        var failedConsumerUrls = failedUrls
            .Select(result => result.Key)
            .ToList();

        return failedConsumerUrls.Count == 1
            ? $"Consumer failed: {failedConsumerUrls[0]}"
            : $"Consumers failed: {string.Join(", ", failedConsumerUrls)}";
    }

    private string EnsureOriginalCorrelationId(ApiMessageTaskEntity entity)
    {
        var originalCorrelationId = CorrelationContextAccessor.NormalizeOrCreate(
            _taskState.State.OriginalCorrelationId ?? entity.OriginalCorrelationId);

        _taskState.State.OriginalCorrelationId = originalCorrelationId;
        entity.OriginalCorrelationId = originalCorrelationId;

        return originalCorrelationId;
    }
    
    private async Task SendGrpcMessageAsync(string fullUrl)
    {
        var (channelUrl, serviceName, methodName) = ParseGrpcUrl(fullUrl);
        var nativeClient = _genericClientFactory.GetClient(channelUrl);

        var requestBytes = Empty.Parser.ParseFrom(Array.Empty<byte>()).ToByteArray();
        _ = await nativeClient.CallRawAsync(serviceName, methodName, requestBytes);
    }

    private static (string ChannelUrl, string ServiceName, string MethodName) ParseGrpcUrl(string url)
    {
        var uri = new Uri(url);
        var scheme = uri.Scheme.ToLowerInvariant();

        if (scheme != "grpc" && scheme != "grpcs")
            throw new ArgumentException($"Unsupported scheme '{uri.Scheme}'. Use 'grpc' or 'grpcs'.");

        var channelScheme = scheme == "grpcs" ? "https" : "http";
        var channelUrl = $"{channelScheme}://{uri.Host}:{uri.Port}";

        var path = uri.AbsolutePath.Trim('/');
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            throw new ArgumentException(
                $"Invalid gRPC URL path: '{uri.AbsolutePath}'. Expected 'serviceFullName/methodName'.");

        return (channelUrl, parts[0], parts[1]);
    }

    private async Task UpdateStateAsync(ApiMessageTaskEntity entity, ApiMessageStateEnum newState)
    {
        _taskState.State.TaskState = newState;
        entity.State = newState;

        if (IsFinishState(newState))
        {
            _taskState.State.FinishTime = DateTime.UtcNow;
            entity.FinishOn = _taskState.State.FinishTime;
            _taskState.State.NextRetryOn = null;
            entity.NextRetryOn = null;
        }

        entity.Attempts = _taskState.State.Attempts;
        entity.MaxAttempts = _taskState.State.MaxAttempts;
        entity.LastError = _taskState.State.LastError;
        entity.TaskName = _taskState.State.TaskName;
        entity.CreatedOn = _taskState.State.CreatedOn;
        entity.StartOn = _taskState.State.StartTime;
        entity.OriginalCorrelationId = _taskState.State.OriginalCorrelationId;

        if (_taskState.State.ConsumerResults != null)
        {
            entity.ConsumerResults = System.Text.Json.JsonSerializer.Serialize(_taskState.State.ConsumerResults);
        }

        await _taskState.WriteStateAsync();
        await SaveTaskEntityAsync(entity);
    }

    private static bool IsFinishState(ApiMessageStateEnum state)
        => state == ApiMessageStateEnum.Done
           || state == ApiMessageStateEnum.Failed
           || state == ApiMessageStateEnum.Disabled
           || state == ApiMessageStateEnum.Suspended
           || state == ApiMessageStateEnum.Cancelled;

    private async Task<ApiMessageTaskEntity> GetOrCreateTaskEntityAsync()
    {
        var repository = _unitOfWork.GetRepository<IApiMessageTaskRepository>();
        var entity = await repository.GetEntityByIdAsync(_taskState.State.TaskId);
        if (entity is not null)
            return entity;

        return new ApiMessageTaskEntity
        {
            Id = _taskState.State.TaskId,
            TaskName = _taskState.State.TaskName,
            State = _taskState.State.TaskState,
            Attempts = _taskState.State.Attempts,
            MaxAttempts = _taskState.State.MaxAttempts,
            LastError = _taskState.State.LastError,
            CreatedOn = _taskState.State.CreatedOn,
            StartOn = _taskState.State.StartTime,
            FinishOn = _taskState.State.FinishTime,
            NextRetryOn = _taskState.State.NextRetryOn,
            OriginalCorrelationId = _taskState.State.OriginalCorrelationId,
            ConsumerResults = _taskState.State.ConsumerResults != null 
                ? System.Text.Json.JsonSerializer.Serialize(_taskState.State.ConsumerResults) 
                : null
        };
    }

    private async Task UpsertTaskEntityAsync()
    {
        var entity = await GetOrCreateTaskEntityAsync();
        await SaveTaskEntityAsync(entity);
    }

    private async Task SaveTaskEntityAsync(ApiMessageTaskEntity entity)
    {
        var repository = _unitOfWork.GetRepository<IApiMessageTaskRepository>();
        var existing = await repository.GetEntityByIdAsync(entity.Id);

        if (existing is null)
        {
            await repository.AddAsync(entity);
        }
        else
        {
            existing.TaskName = entity.TaskName;
            existing.State = entity.State;
            existing.Attempts = entity.Attempts;
            existing.MaxAttempts = entity.MaxAttempts;
            existing.LastError = entity.LastError;
            existing.CreatedOn = entity.CreatedOn;
            existing.StartOn = entity.StartOn;
            existing.FinishOn = entity.FinishOn;
            existing.NextRetryOn = entity.NextRetryOn;
            existing.OriginalCorrelationId = entity.OriginalCorrelationId;
            existing.ConsumerResults = entity.ConsumerResults;

            await repository.UpdateEntityAsync(existing);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task DeactivateGrainAsync(string reminderName)
    {
        var reminder = await this.GetReminder(reminderName);
        if (reminder != null)
            await this.UnregisterReminder(reminder);

        DeactivateOnIdle();
    }
}