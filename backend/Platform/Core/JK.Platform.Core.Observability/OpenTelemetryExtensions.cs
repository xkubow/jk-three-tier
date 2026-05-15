using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace JK.Platform.Core.Observability;

public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Registers OpenTelemetry tracing with ASP.NET Core, HttpClient, and gRPC client instrumentation
    /// when <see cref="OpenTelemetryOptions.Enabled"/> is true. Always binds <see cref="OpenTelemetryOptions"/>
    /// so modules and hosts can inject <c>IOptions&lt;OpenTelemetryOptions&gt;</c>.
    /// </summary>
    public static IServiceCollection AddPlatformOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Action<TracerProviderBuilder>? configureTracerProvider = null,
        Action<MeterProviderBuilder>? configureMeterProvider = null)
    {
        services.AddPlatformOpenTelemetryOptions(configuration);

        var section = configuration.GetSection(OpenTelemetryOptions.SectionName);
        var options = section.Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();
        if (!options.Enabled)
            return services;

        var endpoint =
            configuration["OpenTelemetry:OtlpEndpoint"]
            ?? options.OtlpEndpoint;

        var applicationName = environment.ApplicationName;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(applicationName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.Filter = ctx =>
                            !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddOtlpExporter(o => { o.Endpoint = new Uri(endpoint); });

                configureTracerProvider?.Invoke(tracing);
            })
            .WithMetrics(metrics =>
            {
                configureMeterProvider?.Invoke(metrics);
            });

        return services;
    }
}
