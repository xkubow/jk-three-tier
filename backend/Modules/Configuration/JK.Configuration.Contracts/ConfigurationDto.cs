namespace JK.Configuration.Contracts;

/// <summary>
/// Configuration item DTO. Supports multimarket (MarketCode) and multiservice (ServiceCode) scoping.
/// Null MarketCode/ServiceCode means "applies to all" for that dimension.
/// </summary>
public class ConfigurationDto
{
    public Guid Id { get; set; }
    public string? MarketCode { get; set; }
    public string? ServiceCode { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
