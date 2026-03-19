namespace JK.Configuration.Models;

public class ConfigurationRequest
{
    public string MarketCode { get; set; } = string.Empty;
    public string? MarketArea { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
}