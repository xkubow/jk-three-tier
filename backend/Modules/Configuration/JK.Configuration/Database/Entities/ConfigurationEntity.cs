using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JK.Configuration.Database.Entities;

/// <summary>
/// Multimarket, multiservice configuration entity.
/// MarketCode/ServiceCode null = applies to all markets/services.
/// </summary>
[Table("Configuration")]
public class ConfigurationEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string? MarketCode { get; set; }

    [MaxLength(100)]
    public string? ServiceCode { get; set; }

    [Required]
    [MaxLength(500)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [MaxLength(200)]
    public string? CreatedBy { get; set; }

    [MaxLength(200)]
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
