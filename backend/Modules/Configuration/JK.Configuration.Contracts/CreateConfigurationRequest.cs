namespace JK.Configuration.Contracts;

public class CreateConfigurationRequest
{
    public string? MarketCode { get; set; }
    public string? ServiceCode { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public bool IsList { get; set; }
}
