using JK.Platform.Core.Abstraction;
using JK.Platform.Rest.Swagger.Configurations;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Rest.Swagger;

public class BuilderConfigurator : BuilderConfiguratorBase
{
    public override void ConfigureServices(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddPlatformSwagger(applicationBuilder.Configuration);
    }
}
