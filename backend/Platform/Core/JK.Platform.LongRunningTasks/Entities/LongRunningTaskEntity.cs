using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JK.Platform.LongRunningTasks.Enums;

namespace JK.Platform.LongRunningTasks.Entities;

[Table("LongRunningTask")]
public class LongRunningTaskEntity
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(36)]
    public string? ParentTaskId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TaskName { get; set; } = default!;

    public string? PayloadJson { get; set; }

    [Required]
    [MaxLength(50)]
    public LongRunningTaskStatus Status { get; set; } = LongRunningTaskStatus.Pending;

    public int AttemptCount { get; set; }

    public int MaxAttempts { get; set; } = 3;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime? NextRunAtUtc { get; set; }

    [MaxLength(256)]
    public string? LockedBy { get; set; }

    public DateTime? LockedAtUtc { get; set; }

    public long? TotalItems { get; set; }

    public long ProcessedItems { get; set; }

    public long FailedItems { get; set; }

    public decimal? ProgressPercent { get; set; }

    public int? ChunkNumber { get; set; }

    public int? ChunkSize { get; set; }

    [MaxLength(512)]
    public string? ExternalCursor { get; set; }

    [MaxLength(128)]
    public string? CorrelationId { get; set; }
}
