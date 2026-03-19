using JK.Configuration.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JK.Configuration.Provider;

public static class HostConfigurator
{
    public static IHostApplicationBuilder AddConfigurationServerProvider(
        this IHostApplicationBuilder builder)
    {
        builder.Configuration.Add(new ConfigurationServerSource());
        return builder;
    }

    public static IConfigurationBuilder AddConfigurationServerProvider(
        this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Add(new ConfigurationServerSource());
        return configurationBuilder;
    }
}