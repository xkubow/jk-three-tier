using System.Text.Json;
using JK.Platform.LongRunningTasks.Abstractions;
using JK.Platform.LongRunningTasks.Entities;
using JK.Platform.LongRunningTasks.Enums;
using JK.Platform.LongRunningTasks.Models;

namespace JK.Platform.LongRunningTasks.Services;

public class LongRunningTaskService : ILongRunningTaskService
{
    private readonly ILongRunningTaskRepository _repository;

    public LongRunningTaskService(ILongRunningTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> CreateAsync<TPayload>(
        string taskName,
        TPayload payload,
        int maxAttempts = 3,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var task = new LongRunningTaskEntity
        {
            TaskName = taskName,
            PayloadJson = payload is null ? null : JsonSerializer.Serialize(payload),
            MaxAttempts = maxAttempts,
            Status = LongRunningTaskStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            CorrelationId = correlationId
        };

        await _repository.AddAsync(task, cancellationToken);
        return task.Id;
    }

    public Task<LongRunningTaskEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<LongRunningTaskProgress?> GetProgressAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return null;

        var childStatusCounts = await _repository.CountChildrenByStatusAsync(id, cancellationToken);

        return new LongRunningTaskProgress
        {
            Id = task.Id,
            ParentTaskId = task.ParentTaskId,
            TaskName = task.TaskName,
            Status = task.Status,
            TotalItems = task.TotalItems,
            ProcessedItems = task.ProcessedItems,
            FailedItems = task.FailedItems,
            ProgressPercent = task.ProgressPercent,
            AttemptCount = task.AttemptCount,
            MaxAttempts = task.MaxAttempts,
            ErrorMessage = task.ErrorMessage,
            CreatedAtUtc = task.CreatedAtUtc,
            StartedAtUtc = task.StartedAtUtc,
            CompletedAtUtc = task.CompletedAtUtc,
            CorrelationId = task.CorrelationId,
            ChildStatusCounts = childStatusCounts
        };
    }

    public async Task CancelAsync(string id, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return;

        if (task.Status is LongRunningTaskStatus.Completed or LongRunningTaskStatus.Failed
            or LongRunningTaskStatus.Cancelled)
            return;

        task.Status = LongRunningTaskStatus.Cancelled;
        task.CompletedAtUtc = DateTime.UtcNow;
        task.LockedBy = null;
        task.LockedAtUtc = null;

        await _repository.UpdateAsync(task, cancellationToken);
        await _repository.CancelChildrenAsync(id, cancellationToken);
    }

    public Task RetryFailedChildrenAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.RetryFailedChildrenAsync(id, cancellationToken);
    }
}
