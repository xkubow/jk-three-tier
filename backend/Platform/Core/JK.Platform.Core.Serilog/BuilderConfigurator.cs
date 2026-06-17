using JK.Platform.Core.Abstraction;
using JK.Platform.Core.Serilog.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Core.Serilog;

public class BuilderConfigurator: BuilderConfiguratorBase
{
    public override void AddLogging(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddPlatformSerilog();
        if (applicationBuilder is WebApplicationBuilder webApplicationBuilder)
        {
            webApplicationBuilder.Host.UsePlatformSerilog();
        }

    }
}