using JK.Platform.LongRunningTasks.Entities;
using JK.Platform.LongRunningTasks.Enums;

namespace JK.Platform.LongRunningTasks.Abstractions;

public interface ILongRunningTaskRepository
{
    Task<LongRunningTaskEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<List<LongRunningTaskEntity>> GetPendingTasksAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<List<LongRunningTaskEntity>> ClaimNextPendingTasksAsync(
        int batchSize,
        TimeSpan lockTimeout,
        string lockedBy,
        CancellationToken cancellationToken = default);

    Task<bool> TryClaimAsync(
        string id,
        string lockedBy,
        TimeSpan lockTimeout,
        CancellationToken cancellationToken = default);

    Task AddAsync(LongRunningTaskEntity task, CancellationToken cancellationToken = default);

    Task UpdateAsync(LongRunningTaskEntity task, CancellationToken cancellationToken = default);

    Task<List<LongRunningTaskEntity>> GetChildrenAsync(
        string parentTaskId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<LongRunningTaskStatus, int>> CountChildrenByStatusAsync(
        string parentTaskId,
        CancellationToken cancellationToken = default);

    Task UpdateParentProgressAsync(string parentTaskId, CancellationToken cancellationToken = default);

    Task CreateChildTasksAsync(
        string parentTaskId,
        IReadOnlyList<LongRunningTaskEntity> childTasks,
        CancellationToken cancellationToken = default);

    Task CancelChildrenAsync(
        string parentTaskId,
        CancellationToken cancellationToken = default);

    Task RetryFailedChildrenAsync(
        string parentTaskId,
        CancellationToken cancellationToken = default);
}
