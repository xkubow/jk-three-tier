using JK.Platform.Core.Abstraction;
using JK.Platform.Rest.Server.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Rest.Server;

public class BuilderConfigurator: BuilderConfiguratorBase
{
    public override IMvcBuilder AddRestServer(IHostApplicationBuilder applicationBuilder) => applicationBuilder.Services.AddPlatformRestServer(applicationBuilder.Configuration);
}