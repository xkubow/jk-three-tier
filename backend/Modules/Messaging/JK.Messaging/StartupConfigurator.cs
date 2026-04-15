using JK.Messaging.Configurations;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Messaging;

public sealed class StartupConfigurator : StartupConfiguratorBase<StartupConfigurator>
{
    public override IServiceCollection ConfigureOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigureConfiguration<MessagingConfiguration>(configuration);
        return services;
    }
}
