using System.ComponentModel.DataAnnotations;
using JK.Platform.Persistence.EfCore;

namespace JK.Messaging.Database.Entities;

public class ApiMessageRecurringTaskEntity : EntityBase<string>
{
    [Required]
    [MaxLength(200)]
    public override string Id { get; set; } = null!;

    public string TaskName { get; set; } = default!;

    public string CronExpression { get; set; } = default!;

    public bool IsEnabled { get; set; } = true;

    public DateTime? LastRunAtUtc { get; set; }

    public DateTime? NextRunAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}