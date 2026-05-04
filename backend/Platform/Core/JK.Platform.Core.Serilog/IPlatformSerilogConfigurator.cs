using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions.Core;

namespace JK.Platform.Core.Serilog;

public interface IPlatformSerilogConfigurator
{
    void Initialize(IConfiguration configuration)
    {
    }

    void Configure(LoggerConfiguration loggerConfiguration)
    {
    }

    void Configure(DestructuringOptionsBuilder destructuringOptionsBuilder)
    {
    }
}