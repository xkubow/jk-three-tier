using JK.Platform.Core.Serilog.Configurations;
using Microsoft.Extensions.Configuration;

namespace JK.Platform.Core.Serilog.Extensions;

public static class SerilogConfigurationExtensions
{
    public static bool UsePlatformSerilog(this IConfiguration configuration)
    {
        return configuration
            .GetSection(SerilogOptions.SectionName)
            .Get<SerilogOptions>()?
            .Enabled ?? true;
    }

    public static SerilogOptions GetPlatformSerilogOptions(this IConfiguration configuration)
    {
        return configuration
            .GetSection(SerilogOptions.SectionName)
            .Get<SerilogOptions>() ?? new SerilogOptions();
    }
}