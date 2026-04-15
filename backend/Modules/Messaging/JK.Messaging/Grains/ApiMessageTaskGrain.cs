using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JK.Messaging.Contracts;
using JK.Messaging.Contracts.Enums;
using JK.Messaging.Database.Entities;
using JK.Messaging.Database.Repositories;
using JK.Messaging.Database;
using JK.Messaging.Models;
using JK.Messaging.States;
using JK.Platform.Grpc.Client.Factory;
using JK.Platform.Persistence.EfCore;
using Microsoft.Extensions.Logging;

namespace JK.Messaging.Grains;

public sealed class ApiMessageTaskGrain : Grain, IRemindable, IApiMessageTaskGrain
{
    private readonly IPersistentState<ApiMessageTaskState> _taskState;
    private readonly ILogger<ApiMessageTaskGrain> _logger;
    private readonly IUnitOfWork<MessagingDbContext> _unitOfWork;
    private readonly IGrpcGenericClientFactory _genericClientFactory;

    public ApiMessageTaskGrain(
        [PersistentState("ApiMessageTask", "orleans")] IPersistentState<ApiMessageTaskState> taskState,
        ILogger<ApiMessageTaskGrain> logger,
        IUnitOfWorkFactory<MessagingDbContext> unitOfWorkFactory,
        IGrpcGenericClientFactory genericClientFactory)
    {
        _taskState = taskState;
        _logger = logger;
        _unitOfWork = unitOfWorkFactory.Create();
        _genericClientFactory = genericClientFactory;
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
        _taskState.State.TargetUrl = taskModel.TargetUrl;
        _taskState.State.TaskState = ApiMessageStateEnum.Created;
        _taskState.State.Attempts = 0;
        _taskState.State.MaxAttempts = taskModel.MaxAttempts <= 0 ? 5 : taskModel.MaxAttempts;
        _taskState.State.LastError = null;
        _taskState.State.CreatedOn = DateTime.UtcNow;
        _taskState.State.StartTime = DateTime.UtcNow.Add(delay);
        _taskState.State.FinishTime = null;
        _taskState.State.NextRetryOn = null;

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
            await UpdateStateAsync(entity, ApiMessageStateEnum.Processing);

            await SendGrpcMessageAsync(_taskState.State.TargetUrl);

            _taskState.State.LastError = null;
            _taskState.State.NextRetryOn = null;

            await UpdateStateAsync(entity, ApiMessageStateEnum.Done);
            await DeactivateGrainAsync(reminderName);
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
        entity.TargetUrl = _taskState.State.TargetUrl;
        entity.TaskName = _taskState.State.TaskName;
        entity.CreatedOn = _taskState.State.CreatedOn;
        entity.StartOn = _taskState.State.StartTime;

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
            TargetUrl = _taskState.State.TargetUrl,
            State = _taskState.State.TaskState,
            Attempts = _taskState.State.Attempts,
            MaxAttempts = _taskState.State.MaxAttempts,
            LastError = _taskState.State.LastError,
            CreatedOn = _taskState.State.CreatedOn,
            StartOn = _taskState.State.StartTime,
            FinishOn = _taskState.State.FinishTime,
            NextRetryOn = _taskState.State.NextRetryOn
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
            existing.TargetUrl = entity.TargetUrl;
            existing.State = entity.State;
            existing.Attempts = entity.Attempts;
            existing.MaxAttempts = entity.MaxAttempts;
            existing.LastError = entity.LastError;
            existing.CreatedOn = entity.CreatedOn;
            existing.StartOn = entity.StartOn;
            existing.FinishOn = entity.FinishOn;
            existing.NextRetryOn = entity.NextRetryOn;

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