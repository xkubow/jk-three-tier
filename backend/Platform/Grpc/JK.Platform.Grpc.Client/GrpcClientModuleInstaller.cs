using JK.Platform.Core.Abstraction;
using JK.Platform.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Grpc.Client;

public sealed class GrpcClientModuleInstaller : IModuleInstaller
{
    public string ModuleName => "GrpcClient";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GrpcClientConfiguration>(configuration.GetSection("GrpcClientConfiguration"));
        services.RegisterInjectableServices(typeof(GrpcClientModuleInstaller).Assembly);
    }

    public void RegisterControllers(IMvcBuilder mvcBuilder)
    {
    }

    public void MapGrpcServices(WebApplication app)
    {
    }

    public void MapHealthChecks(WebApplication app)
    {
    }
}
