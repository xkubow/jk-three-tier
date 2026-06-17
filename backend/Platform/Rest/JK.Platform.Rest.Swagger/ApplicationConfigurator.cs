using JK.Platform.Core.Abstraction;
using JK.Platform.Rest.Swagger.Configurations;
using Microsoft.AspNetCore.Builder;

namespace JK.Platform.Rest.Swagger;

public class ApplicationConfigurator: ApplicationConfiguratorBase
{
    public override void ConfigureSwagger(IApplicationBuilder app) => app.UsePlatformSwagger();
}