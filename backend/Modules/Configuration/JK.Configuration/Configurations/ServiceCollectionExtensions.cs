using FluentValidation;
using JK.Configuration.Database;
using JK.Configuration.Database.Repositories;
using JK.Configuration.Services;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Configuration.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurationModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? dbOptions = null)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        return services.AddConfigurationModule(connectionString, dbOptions);
    }

    public static IServiceCollection AddConfigurationModule(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? dbOptions = null)
    {
        services.AddDbContext<ConfigurationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            dbOptions?.Invoke(options);
        });

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ConfigurationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(ConfigurationAssemblyMarker).Assembly);

        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IConfigurationService, ConfigurationService>();

        return services;
    }

    public static IMvcBuilder AddConfigurationModuleControllers(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.ConfigureApplicationPartManager(pm =>
            pm.ApplicationParts.Add(new AssemblyPart(typeof(ConfigurationAssemblyMarker).Assembly)));

        return mvcBuilder;
    }
}
