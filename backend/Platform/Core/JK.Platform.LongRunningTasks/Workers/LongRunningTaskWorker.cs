using System.Diagnostics;
using JK.Platform.LongRunningTasks.Abstractions;
using JK.Platform.LongRunningTasks.Entities;
using JK.Platform.LongRunningTasks.Enums;
using JK.Platform.LongRunningTasks.Observability;
using JK.Platform.LongRunningTasks.Options;
using JK.Platform.LongRunningTasks.Retry;
using JK.Platform.LongRunningTasks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JK.Platform.LongRunningTasks.Workers;

public class LongRunningTaskWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LongRunningTaskWorker> _logger;
    private readonly LongRunningTaskOptions _options;
    private readonly string _workerId = WorkerIdentity.GetWorkerId();

    public LongRunningTaskWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<LongRunningTaskOptions> options,
        ILogger<LongRunningTaskWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Long running task worker started with id {WorkerId}", _workerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unexpected error in long running task worker loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        List<LongRunningTaskEntity> claimed;

        using (var claimScope = _scopeFactory.CreateScope())
        {
            var repository = claimScope.ServiceProvider.GetRequiredService<ILongRunningTaskRepository>();
            claimed = await repository.ClaimNextPendingTasksAsync(
                _options.BatchSize,
                TimeSpan.FromMinutes(_options.LockTimeoutMinutes),
                _workerId,
                cancellationToken);
        }

        if (claimed.Count == 0)
            return;

        var parallelism = Math.Max(1, _options.MaxDegreeOfParallelism);
        using var semaphore = new SemaphoreSlim(parallelism, parallelism);

        var processingTasks = claimed.Select(task =>
            ProcessClaimedTaskAsync(task, semaphore, cancellationToken));

        await Task.WhenAll(processingTasks);
    }

    private async Task ProcessClaimedTaskAsync(
        LongRunningTaskEntity task,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ILongRunningTaskRepository>();
            var handlerRegistry = scope.ServiceProvider.GetRequiredService<LongRunningTaskHandlerRegistry>();
            var metrics = scope.ServiceProvider.GetService<LongRunningTaskMetrics>();

            await ProcessTaskAsync(repository, handlerRegistry, metrics, task, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "Failed to process task {TaskId} {TaskName} parentTaskId {ParentTaskId}",
                task.Id,
                task.TaskName,
                task.ParentTaskId);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task ProcessTaskAsync(
        ILongRunningTaskRepository repository,
        LongRunningTaskHandlerRegistry handlerRegistry,
        LongRunningTaskMetrics? metrics,
        LongRunningTaskEntity task,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Processing task {TaskId} {TaskName} parentTaskId {ParentTaskId} chunk {ChunkNumber}/{ChunkSize} status {Status} attempt {AttemptCount}",
            task.Id,
            task.TaskName,
            task.ParentTaskId,
            task.ChunkNumber,
            task.ChunkSize,
            task.Status,
            task.AttemptCount);

        metrics?.RecordTaskRunning(task.TaskName);

        if (!handlerRegistry.TryGetHandler(task.TaskName, out var handler) || handler is null)
        {
            task.Status = LongRunningTaskStatus.Failed;
            task.ErrorMessage = $"No handler registered for task '{task.TaskName}'.";
            task.CompletedAtUtc = DateTime.UtcNow;
            task.LockedBy = null;
            task.LockedAtUtc = null;
            await repository.UpdateAsync(task, cancellationToken);
            metrics?.RecordTaskFailed(task.TaskName);
            return;
        }

        task.AttemptCount++;

        try
        {
            await handler.ExecuteAsync(task, cancellationToken);

            if (handler is IParentLongRunningTaskHandler)
            {
                var reloaded = await repository.GetByIdAsync(task.Id, cancellationToken);
                if (reloaded is not null)
                {
                    reloaded.LockedBy = null;
                    reloaded.LockedAtUtc = null;
                    await repository.UpdateAsync(reloaded, cancellationToken);
                    task = reloaded;
                }
            }
            else
            {
                task.Status = LongRunningTaskStatus.Completed;
                task.ErrorMessage = null;
                task.CompletedAtUtc = DateTime.UtcNow;
                task.NextRunAtUtc = null;
                task.LockedBy = null;
                task.LockedAtUtc = null;

                await repository.UpdateAsync(task, cancellationToken);

                metrics?.RecordItemsProcessed(task.TaskName, task.ProcessedItems);

                _logger.LogInformation(
                    "Task completed {TaskId} {TaskName} parentTaskId {ParentTaskId} chunk {ChunkNumber} status {Status} attempt {AttemptCount} processedItems {ProcessedItems}",
                    task.Id,
                    task.TaskName,
                    task.ParentTaskId,
                    task.ChunkNumber,
                    task.Status,
                    task.AttemptCount,
                    task.ProcessedItems);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            task.ErrorMessage = ex.Message;

            if (task.AttemptCount < task.MaxAttempts)
            {
                task.Status = LongRunningTaskStatus.Retrying;
                task.NextRunAtUtc = LongRunningTaskRetryPolicy.GetNextRunAtUtc(task.AttemptCount, _options);

                _logger.LogWarning(
                    ex,
                    "Task failed, scheduling retry {TaskId} {TaskName} parentTaskId {ParentTaskId} chunk {ChunkNumber} status {Status} attempt {AttemptCount}",
                    task.Id,
                    task.TaskName,
                    task.ParentTaskId,
                    task.ChunkNumber,
                    task.Status,
                    task.AttemptCount);
            }
            else
            {
                task.Status = LongRunningTaskStatus.Failed;
                task.CompletedAtUtc = DateTime.UtcNow;
                metrics?.RecordTaskFailed(task.TaskName);

                _logger.LogError(
                    ex,
                    "Task failed permanently {TaskId} {TaskName} parentTaskId {ParentTaskId} chunk {ChunkNumber} status {Status} attempt {AttemptCount}",
                    task.Id,
                    task.TaskName,
                    task.ParentTaskId,
                    task.ChunkNumber,
                    task.Status,
                    task.AttemptCount);
            }

            task.LockedBy = null;
            task.LockedAtUtc = null;
            await repository.UpdateAsync(task, cancellationToken);
        }

        if (!string.IsNullOrEmpty(task.ParentTaskId))
            await repository.UpdateParentProgressAsync(task.ParentTaskId, cancellationToken);

        metrics?.RecordTaskDuration(task.TaskName, stopwatch.Elapsed.TotalSeconds);
    }
}
