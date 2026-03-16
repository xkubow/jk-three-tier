using AutoMapper;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.AspNetCore.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Core.AspNetCore.Hosting;

public abstract class StartupBase
{
    protected StartupBase(IConfiguration configuration)
    {
        Configuration = configuration;
        Configurators = CreateStartupConfigurators().ToArray();

        foreach (var configurator in Configurators)
        {
            configurator.Initialize(Configuration);
        }
    }

    protected IConfiguration Configuration { get; }

    protected IReadOnlyList<IStartupConfigurator> Configurators { get; }

    protected virtual IEnumerable<IStartupConfigurator> CreateStartupConfigurators()
        => Array.Empty<IStartupConfigurator>();

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions();
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();

        var mapperConfig = new MapperConfiguration(mapperConfiguration =>
        {
            foreach (var configurator in Configurators)
            {
                if (configurator is IAutoMapperStartupConfigurator autoMapperConfigurator)
                {
                    autoMapperConfigurator.ConfigureAutomapperGlobalMappings(
                        mapperConfiguration,
                        Configuration);
                }
            }
        });
        services.AddSingleton(mapperConfig);
        services.AddScoped<IMapper>(sp => sp.GetRequiredService<MapperConfiguration>().CreateMapper());

        foreach (var configurator in Configurators)
        {
            configurator.ConfigureOptions(services, Configuration);
        }

        IHealthChecksBuilder healthChecks = services.AddHealthChecks();
        foreach (var configurator in Configurators)
        {
            configurator.ConfigureHealthChecks(healthChecks, Configuration);
        }

        foreach (var configurator in Configurators)
        {
            configurator.ConfigureServices(services, Configuration);
        }
    }

    public virtual void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.UseRouting();

        foreach (var configurator in Configurators)
        {
            configurator.ConfigureMiddleware(app, Configuration);
        }

        app.UseEndpoints(endpoints =>
        {
            foreach (var configurator in Configurators)
            {
                configurator.ConfigureEndpoints(endpoints, app, Configuration);
            }
        });
    }
}

