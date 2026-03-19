namespace JK.Configuration.Models;

public class ConfigurationRequest
{
    public string Market { get; set; } = string.Empty;
    public string? MarketArea { get; set; }
    public string Service { get; set; } = string.Empty;
}