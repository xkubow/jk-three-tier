using System.Reflection;
using JK.Platform.Core.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Formatting.Compact;

namespace JK.Platform.Core.Serilog.Extensions;

public static class PlatformSerilogExtensions
{
    public static IHostBuilder UsePlatformSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            // if (!context.Configuration.UsePlatformSerilog())
            //     return;

            var configurators = services
                .GetServices<IPlatformSerilogConfigurator>()
                .ToArray();

            loggerConfiguration.Prepare(
                context.Configuration,
                services,
                configurators);

            foreach (var configurator in configurators)
            {
                configurator.Initialize(context.Configuration);
                configurator.Configure(loggerConfiguration);
            }
        });
    }

    public static WebApplication UsePlatformSerilogRequestLogging(this WebApplication app)
    {
        var options = app.Configuration.GetPlatformSerilogOptions();

        if (!options.Enabled || !options.EnableRequestLogging)
            return app;

        app.UseSerilogRequestLogging(requestOptions =>
        {
            requestOptions.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            requestOptions.GetLevel = (httpContext, elapsed, exception) =>
            {
                if (exception != null)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };

            requestOptions.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var request = httpContext.Request;
                var response = httpContext.Response;
                var host = request.Host.Value ?? string.Empty;
                var path = request.Path.Value ?? string.Empty;
                var queryString = request.QueryString.Value ?? string.Empty;
                var contentType = response.ContentType ?? string.Empty;

                diagnosticContext.Set("Protocol", request.Protocol);
                diagnosticContext.Set("Method", request.Method);
                diagnosticContext.Set("Scheme", request.Scheme);
                diagnosticContext.Set("Host", host);
                diagnosticContext.Set("Path", path);
                diagnosticContext.Set("QueryString", queryString);
                diagnosticContext.Set("StatusCode", response.StatusCode);
                diagnosticContext.Set("ContentType", contentType);
                diagnosticContext.Set(CorrelationIdConstants.LogPropertyName, httpContext.TraceIdentifier);

                var activity = System.Diagnostics.Activity.Current;
                if (activity != null)
                {
                    diagnosticContext.Set("TraceId", activity.TraceId.ToString());
                    activity.SetTag(CorrelationIdConstants.ActivityTagName, httpContext.TraceIdentifier);
                    activity.SetTag("http.protocol", request.Protocol);
                    activity.SetTag("http.scheme", request.Scheme);
                    activity.SetTag("http.host", host);
                    activity.SetTag("http.path", path);
                    activity.SetTag("http.query", queryString);
                    activity.SetTag("http.status_code", response.StatusCode);
                    activity.SetTag("http.content_type", contentType);
                }
            };
        });

        return app;
    }

    private static void Prepare(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        IServiceProvider services,
        IEnumerable<IPlatformSerilogConfigurator> configurators)
    {
        var destructuringOptionsBuilder =
            new DestructuringOptionsBuilder()
                .WithDefaultDestructurers();

        foreach (var configurator in configurators)
        {
            configurator.Configure(destructuringOptionsBuilder);
        }

        var applicationName =
            Assembly.GetEntryAssembly()?.GetName().Name ?? "JK.Application";

        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Orleans", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.WithProperty("ApplicationName", applicationName)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithExceptionDetails(destructuringOptionsBuilder);

        // Keep console logging as a fallback, but avoid duplicating it
        // when the application already defines sinks in Serilog config.
        if (!configuration.GetSection("Serilog:WriteTo").GetChildren().Any())
        {
            loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
        }
    }
}