using System.Text.Json;
using JK.Offer.Database.Repositories;
using JK.Offer.Tasks.External;
using JK.Platform.LongRunningTasks.Abstractions;
using JK.Platform.LongRunningTasks.Entities;
using Microsoft.Extensions.Logging;

namespace JK.Offer.Tasks;

public sealed class SyncOffersChunkHandler : ILongRunningTaskHandler
{
    public const string TaskNameValue = "SyncOffersChunk";
    public string TaskName => TaskNameValue;

    private readonly IExternalOfferStore _externalOfferStore;
    private readonly IOfferRepository _offerRepository;
    private readonly ILogger<SyncOffersChunkHandler> _logger;

    public SyncOffersChunkHandler(
        IExternalOfferStore externalOfferStore,
        IOfferRepository offerRepository,
        ILogger<SyncOffersChunkHandler> logger)
    {
        _externalOfferStore = externalOfferStore;
        _offerRepository = offerRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(LongRunningTaskEntity task, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<SyncOffersChunkPayload>(task.PayloadJson!)
                        ?? throw new InvalidOperationException("SyncOffersChunk payload is missing.");

        var page = await _externalOfferStore.GetPageAsync(
            payload.ExternalStoreCode,
            payload.Cursor,
            payload.Limit,
            cancellationToken);

        var (processed, failed) = await _offerRepository.UpsertExternalOffersBatchAsync(page.Items, cancellationToken);

        task.ProcessedItems = processed;
        task.FailedItems = failed;
        task.ExternalCursor = payload.Cursor;

        _logger.LogInformation(
            "SyncOffersChunk completed {TaskId} parentTaskId {ParentTaskId} chunk {ChunkNumber} processed {ProcessedItems} failed {FailedItems}",
            task.Id,
            task.ParentTaskId,
            task.ChunkNumber,
            processed,
            failed);
    }
}
