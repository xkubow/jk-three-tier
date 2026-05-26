using JK.Offer.Tasks;
using JK.Platform.Core.Correlation;
using JK.Platform.LongRunningTasks.Abstractions;
using JK.Platform.LongRunningTasks.Enums;
using JK.Platform.LongRunningTasks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JK.Offer.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/long-running-tasks")]
public class LongRunningTasksController : ControllerBase
{
    private readonly ILongRunningTaskService _taskService;
    private readonly ICorrelationContextAccessor _correlationContext;

    public LongRunningTasksController(
        ILongRunningTaskService taskService,
        ICorrelationContextAccessor correlationContext)
    {
        _taskService = taskService;
        _correlationContext = correlationContext;
    }

    [HttpPost("test")]
    [ProducesResponseType(typeof(CreateLongRunningTaskResponse), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<CreateLongRunningTaskResponse>> CreateTestTask(
        CancellationToken cancellationToken)
    {
        var taskId = await _taskService.CreateAsync(
            TestLongRunningTaskHandler.TaskNameValue,
            new { Message = "test" },
            correlationId: _correlationContext.GetOrCreateCorrelationId(),
            cancellationToken: cancellationToken);

        return Accepted(new CreateLongRunningTaskResponse { TaskId = taskId });
    }

    [HttpPost("sync-offers")]
    [ProducesResponseType(typeof(CreateLongRunningTaskResponse), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<CreateLongRunningTaskResponse>> SyncOffers(
        [FromBody] SyncOffersRequest request,
        CancellationToken cancellationToken)
    {
        var taskId = await _taskService.CreateAsync(
            SyncOffersHandler.TaskNameValue,
            new SyncOffersPayload
            {
                ExternalStoreCode = request.ExternalStoreCode,
                ChunkSize = request.ChunkSize,
                FullSync = request.FullSync
            },
            correlationId: _correlationContext.GetOrCreateCorrelationId(),
            cancellationToken: cancellationToken);

        return Accepted(new CreateLongRunningTaskResponse { TaskId = taskId });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LongRunningTaskStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LongRunningTaskStatusResponse>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return NotFound();

        return Ok(new LongRunningTaskStatusResponse
        {
            Id = task.Id,
            ParentTaskId = task.ParentTaskId,
            TaskName = task.TaskName,
            Status = task.Status.ToString(),
            TotalItems = task.TotalItems,
            ProcessedItems = task.ProcessedItems,
            FailedItems = task.FailedItems,
            ProgressPercent = task.ProgressPercent,
            AttemptCount = task.AttemptCount,
            ErrorMessage = task.ErrorMessage,
            CorrelationId = task.CorrelationId,
            CreatedAtUtc = task.CreatedAtUtc,
            StartedAtUtc = task.StartedAtUtc,
            CompletedAtUtc = task.CompletedAtUtc
        });
    }

    [HttpGet("{id}/progress")]
    [ProducesResponseType(typeof(LongRunningTaskProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LongRunningTaskProgressResponse>> GetProgress(
        string id,
        CancellationToken cancellationToken)
    {
        var progress = await _taskService.GetProgressAsync(id, cancellationToken);
        if (progress is null)
            return NotFound();

        return Ok(new LongRunningTaskProgressResponse
        {
            Id = progress.Id,
            ParentTaskId = progress.ParentTaskId,
            TaskName = progress.TaskName,
            Status = progress.Status.ToString(),
            TotalItems = progress.TotalItems,
            ProcessedItems = progress.ProcessedItems,
            FailedItems = progress.FailedItems,
            ProgressPercent = progress.ProgressPercent,
            AttemptCount = progress.AttemptCount,
            MaxAttempts = progress.MaxAttempts,
            ErrorMessage = progress.ErrorMessage,
            CorrelationId = progress.CorrelationId,
            CreatedAtUtc = progress.CreatedAtUtc,
            StartedAtUtc = progress.StartedAtUtc,
            CompletedAtUtc = progress.CompletedAtUtc,
            ChildStatusCounts = progress.ChildStatusCounts.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value)
        });
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(string id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return NotFound();

        await _taskService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/retry-failed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryFailed(string id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return NotFound();

        await _taskService.RetryFailedChildrenAsync(id, cancellationToken);
        return NoContent();
    }
}

public sealed class SyncOffersRequest
{
    public string ExternalStoreCode { get; set; } = "StoreA";

    public int ChunkSize { get; set; } = 1000;

    public bool FullSync { get; set; } = true;
}

public sealed class CreateLongRunningTaskResponse
{
    public string TaskId { get; set; } = default!;
}

public sealed class LongRunningTaskStatusResponse
{
    public string Id { get; set; } = default!;

    public string? ParentTaskId { get; set; }

    public string TaskName { get; set; } = default!;

    public string Status { get; set; } = default!;

    public long? TotalItems { get; set; }

    public long ProcessedItems { get; set; }

    public long FailedItems { get; set; }

    public decimal? ProgressPercent { get; set; }

    public int AttemptCount { get; set; }

    public string? ErrorMessage { get; set; }

    public string? CorrelationId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
}

public sealed class LongRunningTaskProgressResponse
{
    public string Id { get; set; } = default!;

    public string? ParentTaskId { get; set; }

    public string TaskName { get; set; } = default!;

    public string Status { get; set; } = default!;

    public long? TotalItems { get; set; }

    public long ProcessedItems { get; set; }

    public long FailedItems { get; set; }

    public decimal? ProgressPercent { get; set; }

    public int AttemptCount { get; set; }

    public int MaxAttempts { get; set; }

    public string? ErrorMessage { get; set; }

    public string? CorrelationId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public Dictionary<string, int> ChildStatusCounts { get; set; } = new();
}
