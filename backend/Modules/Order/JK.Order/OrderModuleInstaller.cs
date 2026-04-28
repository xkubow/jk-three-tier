using FluentValidation;
using JK.Platform.Database.Migrations;
using JK.Order.Configurations;
using JK.Order.Database;
using JK.Order.Grpc;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.AspNetCore.Discovery;
using JK.Platform.Core.DependencyInjection;
using JK.Platform.Persistence.EfCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Order;

public class OrderModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Order";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var localAssembly = typeof(OrderAssemblyMarker).Assembly;
        var databaseAssembly = typeof(OrderDatabaseMarker).Assembly;
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("DefaultConnection configuration is missing or empty.");

        services.AddDbContext<OrderDbContext>(options => { options.UseNpgsql(connectionString); });

        services.AddBackendMigrations(connectionString, localAssembly, databaseAssembly);


        services.AddAutoMapper(localAssembly);
        services.AddValidatorsFromAssembly(localAssembly);

        // Discover and register all injectable services from JK.* assemblies
        var domainAssemblies = DomainDiscovery.FindDomainAssemblies();
        foreach (var assembly in domainAssemblies)
        {
            services.RegisterInjectableServices(assembly);
        }
        services.RegisterInjectableServices(localAssembly);
        services.AddUnitOfWork();
    }

    public void RegisterControllers(IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(OrderAssemblyMarker).Assembly);
    }

    public void MapGrpcServices(WebApplication app)
    {
        app.MapGrpcService<OrderGrpcService>();
    }

    public void MapHealthChecks(WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

        app.MapHealthChecks("/health");
    }
}