namespace JK.Configuration.Contracts;

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
    public bool IsList { get; set; }
}
