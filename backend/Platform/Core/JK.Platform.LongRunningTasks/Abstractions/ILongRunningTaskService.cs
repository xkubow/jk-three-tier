using JK.Platform.LongRunningTasks.Entities;
using JK.Platform.LongRunningTasks.Models;

namespace JK.Platform.LongRunningTasks.Abstractions;

public interface ILongRunningTaskService
{
    Task<string> CreateAsync<TPayload>(
        string taskName,
        TPayload payload,
        int maxAttempts = 3,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    Task<LongRunningTaskEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<LongRunningTaskProgress?> GetProgressAsync(string id, CancellationToken cancellationToken = default);

    Task CancelAsync(string id, CancellationToken cancellationToken = default);

    Task RetryFailedChildrenAsync(string id, CancellationToken cancellationToken = default);
}
