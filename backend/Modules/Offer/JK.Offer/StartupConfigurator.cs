using JK.Offer.Configurations;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Offer;

public sealed class StartupConfigurator : StartupConfiguratorBase<StartupConfigurator>
{
    public override IServiceCollection ConfigureOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigureConfiguration<OfferConfiguration>(configuration);
        return services;
    }
}
