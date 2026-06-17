using JK.Platform.Core.Abstraction;
using JK.Platform.Http.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Http;

public class BuilderConfigurator: BuilderConfiguratorBase
{
    public override void ConfigureServices(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddPlatformCors(applicationBuilder.Configuration);
        applicationBuilder.Services.AddEndpointsApiExplorer();
        applicationBuilder.Services.AddHealthChecks();
    }

    public override void AddWebHostConfiguration(IHostApplicationBuilder applicationBuilder)
    {
        if (applicationBuilder is WebApplicationBuilder webApplicationBuilder && applicationBuilder.Environment.IsEnvironment("K8s"))
        {
            webApplicationBuilder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8080, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });

                options.ListenAnyIP(8081, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
            });
        }
    }
}