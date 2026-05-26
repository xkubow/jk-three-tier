using JK.Platform.LongRunningTasks.Enums;

namespace JK.Platform.LongRunningTasks.Models;

public sealed class LongRunningTaskProgress
{
    public string Id { get; set; } = default!;

    public string? ParentTaskId { get; set; }

    public string TaskName { get; set; } = default!;

    public LongRunningTaskStatus Status { get; set; }

    public long? TotalItems { get; set; }

    public long ProcessedItems { get; set; }

    public long FailedItems { get; set; }

    public decimal? ProgressPercent { get; set; }

    public int AttemptCount { get; set; }

    public int MaxAttempts { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public string? CorrelationId { get; set; }

    public IReadOnlyDictionary<LongRunningTaskStatus, int> ChildStatusCounts { get; set; } =
        new Dictionary<LongRunningTaskStatus, int>();
}
