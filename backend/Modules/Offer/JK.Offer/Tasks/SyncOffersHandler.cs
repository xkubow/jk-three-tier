using System.Text.Json;
using JK.Offer.Tasks.External;
using JK.Platform.LongRunningTasks.Abstractions;
using JK.Platform.LongRunningTasks.Entities;
using JK.Platform.LongRunningTasks.Enums;
using Microsoft.Extensions.Logging;

namespace JK.Offer.Tasks;

public sealed class SyncOffersHandler : IParentLongRunningTaskHandler
{
    public const string TaskNameValue = "SyncOffers";
    public string TaskName => TaskNameValue;

    private readonly IExternalOfferStore _externalOfferStore;
    private readonly ILongRunningTaskRepository _repository;
    private readonly ILogger<SyncOffersHandler> _logger;

    public SyncOffersHandler(
        IExternalOfferStore externalOfferStore,
        ILongRunningTaskRepository repository,
        ILogger<SyncOffersHandler> logger)
    {
        _externalOfferStore = externalOfferStore;
        _repository = repository;
        _logger = logger;
    }

    public async Task ExecuteAsync(LongRunningTaskEntity task, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<SyncOffersPayload>(task.PayloadJson!)
                        ?? throw new InvalidOperationException("SyncOffers payload is missing.");

        if (string.IsNullOrWhiteSpace(payload.ExternalStoreCode))
            throw new InvalidOperationException("ExternalStoreCode is required.");

        var chunkSize = Math.Clamp(payload.ChunkSize, 500, 5000);
        var totalCount = await _externalOfferStore.GetTotalCountAsync(payload.ExternalStoreCode, cancellationToken);

        _logger.LogInformation(
            "SyncOffers orchestration started {TaskId} store {ExternalStoreCode} totalItems {TotalItems} chunkSize {ChunkSize}",
            task.Id,
            payload.ExternalStoreCode,
            totalCount,
            chunkSize);

        task.TotalItems = totalCount;
        task.Status = LongRunningTaskStatus.Running;

        var childTasks = new List<LongRunningTaskEntity>();
        var chunkCount = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)chunkSize);

        for (var chunkNumber = 1; chunkNumber <= chunkCount; chunkNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var offset = (chunkNumber - 1) * chunkSize;
            var remaining = totalCount - offset;
            var currentChunkSize = (int)Math.Min(chunkSize, remaining);
            var cursor = offset == 0 ? null : offset.ToString();

            childTasks.Add(new LongRunningTaskEntity
            {
                TaskName = SyncOffersChunkHandler.TaskNameValue,
                PayloadJson = JsonSerializer.Serialize(new SyncOffersChunkPayload
                {
                    ExternalStoreCode = payload.ExternalStoreCode,
                    Cursor = cursor,
                    Limit = currentChunkSize
                }),
                Status = LongRunningTaskStatus.Pending,
                MaxAttempts = task.MaxAttempts,
                ChunkNumber = chunkNumber,
                ChunkSize = currentChunkSize,
                ExternalCursor = cursor,
                CorrelationId = task.CorrelationId,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _repository.CreateChildTasksAsync(task.Id, childTasks, cancellationToken);

        task.ProgressPercent = 0;
        await _repository.UpdateAsync(task, cancellationToken);

        _logger.LogInformation(
            "SyncOffers orchestration created {ChildCount} child tasks for {TaskId}",
            childTasks.Count,
            task.Id);
    }
}
