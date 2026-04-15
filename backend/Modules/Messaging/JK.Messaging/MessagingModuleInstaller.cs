using FluentValidation;
using JK.Backend.Migrations;
using JK.Messaging.Configurations;
using JK.Messaging.Database;
using JK.Messaging.Grpc;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.DependencyInjection;
using JK.Platform.Persistence.EfCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Messaging;

public class MessagingModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Messaging";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(MessagingAssemblyMarker).Assembly;
        var databaseAssembly = typeof(MessagingDatabaseMarker).Assembly;
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("DefaultConnection configuration is missing or empty.");

        services.AddDbContext<MessagingDbContext>(options => { options.UseNpgsql(connectionString); });

        services.AddBackendMigrations(connectionString, assembly, databaseAssembly);

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.RegisterInjectableServices(assembly);
        services.AddUnitOfWork();
    }

    public void RegisterControllers(IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(MessagingAssemblyMarker).Assembly);
    }

    public void MapGrpcServices(WebApplication app)
    {
        app.MapGrpcService<MessagingGrpcService>();
    }

    public void MapHealthChecks(WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

        app.MapHealthChecks("/health");
    }
}
