using JK.Platform.Core.Abstraction;
using JK.Platform.Grpc.Server.Extensions;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Grpc.Server;

public class BuilderConfigurator : BuilderConfiguratorBase
{
    public override void ConfigureServices(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddGrpcPlatform();
    }
}
