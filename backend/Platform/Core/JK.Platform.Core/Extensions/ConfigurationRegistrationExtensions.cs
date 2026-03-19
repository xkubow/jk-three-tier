using JK.Platform.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JK.Platform.Core.Extensions;

public static class ConfigurationRegistrationExtensions
{
    public static IServiceCollection ConfigureConfiguration<TConfiguration>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TConfiguration : class, IAppConfiguration, new()
    {
        var instance = new TConfiguration();
        var section = configuration.GetSection(instance.SectionName);

        services.Configure<TConfiguration>(section);

        // Optional: also expose resolved value directly
        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<TConfiguration>>().Value);

        return services;
    }
}