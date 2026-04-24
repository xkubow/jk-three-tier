namespace JK.Configuration.Contracts;

public class UpdateConfigurationRequest
{
    public string Value { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public bool IsList { get; set; }
}
