using JK.Platform.Core.Serilog.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.Serilog.Extensions;

public static class PlatformSerilogServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformSerilog(this IServiceCollection services)
    {
        services
            .AddOptions<SerilogOptions>()
            .BindConfiguration(SerilogOptions.SectionName);

        return services;
    }

    public static IServiceCollection AddPlatformSerilogConfigurator<TConfigurator>(
        this IServiceCollection services)
        where TConfigurator : class, IPlatformSerilogConfigurator
    {
        services.AddSingleton<IPlatformSerilogConfigurator, TConfigurator>();
        return services;
    }
}