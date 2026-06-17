using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Core.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions.Core;

namespace JK.Configuration.Configurations;

[Injectable(ServiceLifetime.Singleton)]
public sealed class ConfigurationSerilogConfigurator : IPlatformSerilogConfigurator
{
    public void Initialize(IConfiguration configuration)
    {
    }

    public void Configure(LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .MinimumLevel.Override("JK.Configuration", LogEventLevel.Debug);
    }

    public void Configure(DestructuringOptionsBuilder destructuringOptionsBuilder)
    {
    }
}