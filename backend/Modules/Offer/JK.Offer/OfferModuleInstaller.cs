using FluentValidation;
using JK.Backend.Migrations;
using JK.Offer.Configurations;
using JK.Offer.Database;
using JK.Offer.Grpc;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.DependencyInjection;
using JK.Platform.Http.Extensions;
using JK.Platform.Persistence.EfCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Offer;

public class OfferModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Offer";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(OfferAssemblyMarker).Assembly;
        var databaseAssembly = typeof(OfferDatabaseMarker).Assembly;
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("DefaultConnection configuration is missing or empty.");

        services.AddDbContext<OfferDbContext>(options => { options.UseNpgsql(connectionString); });

        services.AddBackendMigrations(connectionString, assembly, databaseAssembly);

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.RegisterInjectableServices(assembly);
        services.AddUnitOfWork();
    }

    public void RegisterControllers(IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(OfferAssemblyMarker).Assembly);
    }

    public void MapGrpcServices(WebApplication app)
    {
        app.MapGrpcService<OfferGrpcService>();
    }

    public void MapHealthChecks(WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

        app.MapHealthChecks("/health");
    }
}
