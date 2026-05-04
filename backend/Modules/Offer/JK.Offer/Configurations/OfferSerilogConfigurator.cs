using JK.Platform.Core.Serilog;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions.Core;

namespace JK.Offer.Configurations;

public sealed class OfferSerilogConfigurator : IPlatformSerilogConfigurator
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