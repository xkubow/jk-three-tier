using JK.Platform.Core.Serilog;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions.Core;

namespace JK.Configuration.Configurations;

public sealed class SerilogConfigurator : IPlatformSerilogConfigurator
{
    public void Initialize(IConfiguration configuration)
    {
    }

    public void Configure(LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .MinimumLevel.Override("JK.Messaging", LogEventLevel.Debug)
            .MinimumLevel.Override("Orleans", LogEventLevel.Information);
    }

    public void Configure(DestructuringOptionsBuilder destructuringOptionsBuilder)
    {
    }
}