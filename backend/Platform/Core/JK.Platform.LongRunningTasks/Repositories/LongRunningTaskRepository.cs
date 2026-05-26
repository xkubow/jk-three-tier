using JK.Platform.LongRunningTasks.Abstractions;
using JK.Platform.LongRunningTasks.Entities;
using JK.Platform.LongRunningTasks.Enums;
using Microsoft.EntityFrameworkCore;

namespace JK.Platform.LongRunningTasks.Repositories;

public class LongRunningTaskRepository<TContext> : ILongRunningTaskRepository
    where TContext : DbContext, ILongRunningTasksDbContext
{
    private const int ChildInsertBatchSize = 500;

    private readonly TContext _context;

    public LongRunningTaskRepository(TContext context)
    {
        _context = context;
    }

    public Task<LongRunningTaskEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _context.LongRunningTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<LongRunningTaskEntity>> GetPendingTasksAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.LongRunningTasks
            .AsNoTracking()
            .Where(t => (t.Status == LongRunningTaskStatus.Pending
                         || t.Status == LongRunningTaskStatus.Retrying)
                        && (t.NextRunAtUtc == null || t.NextRunAtUtc <= now))
            .OrderBy(t => t.ParentTaskId == null ? 0 : 1)
            .ThenBy(t => t.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LongRunningTaskEntity>> ClaimNextPendingTasksAsync(
        int batchSize,
        TimeSpan lockTimeout,
        string lockedBy,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var lockExpiry = now - lockTimeout;

        var candidateIds = await _context.LongRunningTasks
            .AsNoTracking()
            .Where(t =>
                ((t.Status == LongRunningTaskStatus.Pending
                  || t.Status == LongRunningTaskStatus.Retrying)
                 && (t.NextRunAtUtc == null || t.NextRunAtUtc <= now))
                || (t.Status == LongRunningTaskStatus.Running
                    && t.LockedAtUtc != null
                    && t.LockedAtUtc < lockExpiry))
            .OrderBy(t => t.ParentTaskId == null ? 0 : 1)
            .ThenBy(t => t.CreatedAtUtc)
            .Take(batchSize)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var claimed = new List<LongRunningTaskEntity>(candidateIds.Count);

        foreach (var id in candidateIds)
        {
            if (!await TryClaimAsync(id, lockedBy, lockTimeout, cancellationToken))
                continue;

            var task = await GetByIdAsync(id, cancellationToken);
            if (task is not null)
                claimed.Add(task);
        }

        return claimed;
    }

    public async Task<bool> TryClaimAsync(
        string id,
        string lockedBy,
        TimeSpan lockTimeout,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var lockExpiry = now - lockTimeout;

        var rowsAffected = await _context.LongRunningTasks
            .Where(t => t.Id == id)
            .Where(t =>
                ((t.Status == LongRunningTaskStatus.Pending
                  || t.Status == LongRunningTaskStatus.Retrying)
                 && (t.NextRunAtUtc == null || t.NextRunAtUtc <= now))
                || (t.Status == LongRunningTaskStatus.Running
                    && t.LockedAtUtc != null
                    && t.LockedAtUtc < lockExpiry))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.Status, LongRunningTaskStatus.Running)
                    .SetProperty(t => t.LockedBy, lockedBy)
                    .SetProperty(t => t.LockedAtUtc, now)
                    .SetProperty(
                        t => t.StartedAtUtc,
                        t => t.StartedAtUtc ?? now),
                cancellationToken);

        return rowsAffected > 0;
    }

    public async Task AddAsync(LongRunningTaskEntity task, CancellationToken cancellationToken = default)
    {
        await _context.LongRunningTasks.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LongRunningTaskEntity task, CancellationToken cancellationToken = default)
    {
        _context.LongRunningTasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<LongRunningTaskEntity>> GetChildrenAsync(
        string parentTaskId,
        CancellationToken cancellationToken = default)
    {
        return _context.LongRunningTasks
            .AsNoTracking()
            .Where(t => t.ParentTaskId == parentTaskId)
            .OrderBy(t => t.ChunkNumber)
            .ThenBy(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<LongRunningTaskStatus, int>> CountChildrenByStatusAsync(
        string parentTaskId,
        CancellationToken cancellationToken = default)
    {
        var counts = await _context.LongRunningTasks
            .AsNoTracking()
            .Where(t => t.ParentTaskId == parentTaskId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task UpdateParentProgressAsync(string parentTaskId, CancellationToken cancellationToken = default)
    {
        var parent = await _context.LongRunningTasks
            .FirstOrDefaultAsync(t => t.Id == parentTaskId, cancellationToken);

        if (parent is null)
            return;

        var children = await _context.LongRunningTasks
            .AsNoTracking()
            .Where(t => t.ParentTaskId == parentTaskId)
            .ToListAsync(cancellationToken);

        if (children.Count == 0)
            return;

        parent.ProcessedItems = children.Sum(c => c.ProcessedItems);
        parent.FailedItems = children.Sum(c => c.FailedItems);

        var completedChunks = children.Count(c => c.Status == LongRunningTaskStatus.Completed);
        var totalChunks = children.Count;

        if (parent.TotalItems is > 0)
        {
            parent.ProgressPercent = Math.Round(
                (decimal)parent.ProcessedItems / parent.TotalItems.Value * 100m,
                4);
        }
        else
        {
            parent.ProgressPercent = Math.Round(
                (decimal)completedChunks / totalChunks * 100m,
                4);
        }

        var terminalStatuses = new[]
        {
            LongRunningTaskStatus.Completed,
            LongRunningTaskStatus.Failed,
            LongRunningTaskStatus.Cancelled
        };

        if (!children.All(c => terminalStatuses.Contains(c.Status)))
        {
            parent.Status = LongRunningTaskStatus.Running;
            parent.LockedBy = null;
            parent.LockedAtUtc = null;
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var failedCount = children.Count(c => c.Status == LongRunningTaskStatus.Failed);
        var completedCount = children.Count(c => c.Status == LongRunningTaskStatus.Completed);
        var cancelledCount = children.Count(c => c.Status == LongRunningTaskStatus.Cancelled);

        parent.Status = failedCount switch
        {
            0 when cancelledCount == 0 => LongRunningTaskStatus.Completed,
            0 => LongRunningTaskStatus.Cancelled,
            _ when completedCount > 0 || cancelledCount > 0 => LongRunningTaskStatus.PartiallyCompleted,
            _ => LongRunningTaskStatus.Failed
        };

        parent.CompletedAtUtc = DateTime.UtcNow;
        parent.LockedBy = null;
        parent.LockedAtUtc = null;
        parent.ErrorMessage = failedCount > 0
            ? $"{failedCount} of {totalChunks} child tasks failed."
            : null;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateChildTasksAsync(
        string parentTaskId,
        IReadOnlyList<LongRunningTaskEntity> childTasks,
        CancellationToken cancellationToken = default)
    {
        if (childTasks.Count == 0)
            return;

        for (var offset = 0; offset < childTasks.Count; offset += ChildInsertBatchSize)
        {
            var batch = childTasks.Skip(offset).Take(ChildInsertBatchSize).ToList();

            foreach (var child in batch)
            {
                child.ParentTaskId = parentTaskId;
                if (child.Status == default)
                    child.Status = LongRunningTaskStatus.Pending;
            }

            await _context.LongRunningTasks.AddRangeAsync(batch, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CancelChildrenAsync(string parentTaskId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        await _context.LongRunningTasks
            .Where(t => t.ParentTaskId == parentTaskId)
            .Where(t => t.Status == LongRunningTaskStatus.Pending
                        || t.Status == LongRunningTaskStatus.Retrying)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.Status, LongRunningTaskStatus.Cancelled)
                    .SetProperty(t => t.CompletedAtUtc, now)
                    .SetProperty(t => t.LockedBy, (string?)null)
                    .SetProperty(t => t.LockedAtUtc, (DateTime?)null),
                cancellationToken);
    }

    public async Task RetryFailedChildrenAsync(string parentTaskId, CancellationToken cancellationToken = default)
    {
        await _context.LongRunningTasks
            .Where(t => t.ParentTaskId == parentTaskId)
            .Where(t => t.Status == LongRunningTaskStatus.Failed)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.Status, LongRunningTaskStatus.Pending)
                    .SetProperty(t => t.ErrorMessage, (string?)null)
                    .SetProperty(t => t.NextRunAtUtc, (DateTime?)null)
                    .SetProperty(t => t.CompletedAtUtc, (DateTime?)null)
                    .SetProperty(t => t.LockedBy, (string?)null)
                    .SetProperty(t => t.LockedAtUtc, (DateTime?)null),
                cancellationToken);

        var parent = await _context.LongRunningTasks
            .FirstOrDefaultAsync(t => t.Id == parentTaskId, cancellationToken);

        if (parent is null)
            return;

        if (parent.Status is LongRunningTaskStatus.Failed
            or LongRunningTaskStatus.PartiallyCompleted
            or LongRunningTaskStatus.Completed
            or LongRunningTaskStatus.Cancelled)
        {
            parent.Status = LongRunningTaskStatus.Running;
            parent.CompletedAtUtc = null;
            parent.ErrorMessage = null;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

}
