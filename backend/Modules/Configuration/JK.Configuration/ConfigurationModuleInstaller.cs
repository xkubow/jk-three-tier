using FluentValidation;
using JK.Configuration.Configurations;
using JK.Configuration.Database;
using JK.Configuration.Endpoints.GrpcPorts;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.AspNetCore.Abstractions;
using JK.Platform.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Configuration;

public class ConfigurationModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Configuration";
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(ConfigurationModuleInstaller).Assembly;

        services.AddDbContext<ConfigurationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.RegisterInjectableServices(assembly);
    }

    public void RegisterControllers(IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(ConfigurationModuleInstaller).Assembly);
    }

    public void MapGrpcServices(WebApplication app)
    {
        app.MapGrpcService<ConfigurationGrpcPort>();
    }
}
