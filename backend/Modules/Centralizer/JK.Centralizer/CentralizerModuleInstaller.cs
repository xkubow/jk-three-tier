using AutoMapper;
using JK.Order.Client.Grpc;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Centralizer;

public class CentralizerModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Centralizer";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(CentralizerAssemblyMarker).Assembly;

        services.AddAutoMapper(assembly);
        
        // Register services from this assembly marked with [Injectable]
        services.RegisterInjectableServices(assembly);
        
        // Register gRPC Clients for other modules
        var orderServiceUrl = configuration["GrpcClients:OrderService:Url"];
        if (!string.IsNullOrEmpty(orderServiceUrl))
        {
            services.AddScoped<IOrderGrpcClient>(_ => new OrderGrpcClient(orderServiceUrl));
        }
    }

    public void RegisterControllers(IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(CentralizerAssemblyMarker).Assembly);
    }

    public void MapGrpcServices(WebApplication app)
    {
        // Centralizer doesn't provide Grpc services
    }

    public void MapHealthChecks(WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });
        app.MapHealthChecks("/health");
    }
}
