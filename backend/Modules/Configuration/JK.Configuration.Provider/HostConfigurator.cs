using Grpc.Core.Interceptors;
using Grpc.Net.Client.Balancer;
using JK.Configuration.Proto;
using JK.Platform.Core.DependencyInjection;
using JK.Platform.Grpc.Client;
using JK.Platform.Grpc.Client.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JK.Configuration.Provider;

public static class HostConfigurator
{
    public static IHostApplicationBuilder AddConfigurationServerProvider(this IHostApplicationBuilder builder)
    {
        var grpcClientFactory = CreateConfigurationGrpcClientFactory(builder.Configuration);

        builder.Configuration.Add(new ConfigurationServerSource()
        {
            GrpcClientFactory = grpcClientFactory
        });
        return builder;
    }

    public static IConfigurationBuilder AddConfigurationServerProvider(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Add(new ConfigurationServerSource());
        return configurationBuilder;
    }

    private static IGrpcClientFactory<ConfigurationGrpc.ConfigurationGrpcClient> CreateConfigurationGrpcClientFactory(
        IConfiguration configuration)
    {
        var grpcClientConfiguration = configuration
                                          .GetSection("GrpcClientConfiguration")
                                          .Get<GrpcClientConfiguration>()
                                      ?? new GrpcClientConfiguration();

        var services = new ServiceCollection();

        services.AddLogging(logging =>
        {
            logging.AddConfiguration(configuration.GetSection("Logging"));
            logging.AddConsole();
        });

        services.AddSingleton<IOptionsMonitor<GrpcClientConfiguration>>(new GrpcClientConfigurationOptionsMonitor(grpcClientConfiguration));

        services.AddSingleton<ResolverFactory>(new DnsResolverFactory(TimeSpan.FromSeconds(5)));

        services.AddSingleton<IEnumerable<Interceptor>>(_ => Array.Empty<Interceptor>());

        services.RegisterInjectableServices(typeof(GrpcChannelFactory).Assembly);

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IGrpcClientFactory<ConfigurationGrpc.ConfigurationGrpcClient>>();
    }
}