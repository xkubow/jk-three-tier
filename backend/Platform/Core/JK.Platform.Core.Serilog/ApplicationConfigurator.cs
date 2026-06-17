using JK.Platform.Core.Abstraction;
using JK.Platform.Core.Serilog.Extensions;
using Microsoft.AspNetCore.Builder;

namespace JK.Platform.Core.Serilog;

public class ApplicationConfigurator: ApplicationConfiguratorBase
{
    public override void ConfigureSerilog(IApplicationBuilder app)
    {
        if(app is WebApplication webApplication)
            webApplication.UsePlatformSerilogRequestLogging();
    }
}