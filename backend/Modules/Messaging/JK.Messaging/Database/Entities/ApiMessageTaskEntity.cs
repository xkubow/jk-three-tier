using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JK.Messaging.Contracts.Enums;
using JK.Platform.Persistence.EfCore;

namespace JK.Messaging.Database.Entities;

[Table("ApiMessageTask")]
public class ApiMessageTaskEntity : EntityBase<string>
{
    [Required]
    [MaxLength(200)]
    public override string Id { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string TaskName { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string TargetUrl { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public ApiMessageStateEnum State { get; set; } = ApiMessageStateEnum.Waiting;

    public int Attempts { get; set; }

    public int MaxAttempts { get; set; } = 5;

    public string? LastError { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? StartOn { get; set; }

    public DateTime? FinishOn { get; set; }

    public DateTime? NextRetryOn { get; set; }
    [Column(TypeName = "jsonb")]
    public string? ConsumerResults { get; set; }
}