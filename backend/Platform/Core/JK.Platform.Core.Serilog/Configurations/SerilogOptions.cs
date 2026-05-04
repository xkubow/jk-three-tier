namespace JK.Platform.Core.Serilog.Configurations;

public sealed class SerilogOptions
{
    public const string SectionName = "Platform:Serilog";

    public bool Enabled { get; set; } = true;

    public bool EnableRequestLogging { get; set; } = true;
}