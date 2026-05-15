using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.Observability;

/// <summary>
/// Lets modules or hosts bind observability settings without starting tracing (for tests and minimal hosts).
/// Full APIs should call <see cref="OpenTelemetryExtensions.AddPlatformOpenTelemetry"/>.
/// </summary>
public static class OpenTelemetryModuleExtensions
{
    public static IServiceCollection AddPlatformOpenTelemetryOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<OpenTelemetryOptions>()
            .BindConfiguration(OpenTelemetryOptions.SectionName);

        return services;
    }
}
