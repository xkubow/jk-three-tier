using System.ComponentModel.DataAnnotations;
using JK.Platform.Persistence.EfCore;

namespace JK.Messaging.Database.Entities;

public class ApiMessageRecurringTaskModel : EntityBase<string>
{
    [Required]
    [MaxLength(200)]
    public override string Id { get; set; } = null!;

    public string Name { get; set; } = default!;

    // Example: "*/5 * * * *" = every 5 minutes
    public string CronExpression { get; set; } = default!;

    public bool IsEnabled { get; set; }

    public DateTime? LastRunAtUtc { get; set; }

    public DateTime? NextRunAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}