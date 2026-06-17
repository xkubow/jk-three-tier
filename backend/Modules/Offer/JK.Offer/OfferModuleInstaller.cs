using FluentValidation;
using JK.Offer.Configurations;
using JK.Offer.Database;
using JK.Offer.Grpc;
using JK.Offer.Tasks.External;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.DependencyInjection;
using JK.Platform.Core.Observability;
using JK.Platform.Core.Serilog.Extensions;
using JK.Platform.Database.Migrations;
using JK.Platform.Grpc.Server.Extensions;
using JK.Platform.Http.Configurations;
using JK.Platform.LongRunningTasks.Extensions;
using JK.Platform.Persistence.EfCore.Extensions;
using JK.Platform.Rest.Swagger.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Offer;

public class OfferModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Offer";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment builderEnvironment)
    {
        var assembly = typeof(OfferAssemblyMarker).Assembly;
        var databaseAssembly = typeof(OfferDatabaseMarker).Assembly;
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("DefaultConnection configuration is missing or empty.");

        services.AddPlatformSerilogConfigurator<OfferSerilogConfigurator>();
        services.AddDbContext<OfferDbContext>(options => { options.UseNpgsql(connectionString); });

        services.AddBackendMigrations(connectionString, databaseAssembly);

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.AddUnitOfWork();
        services.AddLongRunningTasks<OfferDbContext>(configuration, assembly);

        services.Configure<FakeExternalOfferStoreOptions>(
            configuration.GetSection(FakeExternalOfferStoreOptions.SectionName));
        services.AddSingleton<IExternalOfferStore, FakeExternalOfferStore>();

        services.AddPlatformOpenTelemetry(configuration, builderEnvironment, builder =>
        {
            builder.AddSource(Instrumentation.ActivitySource.Name);
        },
        metricsBuilder =>
        {
            metricsBuilder.AddMeter(Instrumentation.Meter.Name);
            metricsBuilder.AddMeter("JK.Platform.LongRunningTasks");
        });
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
