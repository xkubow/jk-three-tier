using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.Abstraction;

public abstract class StartupConfiguratorBase<TSelf>
    where TSelf : StartupConfiguratorBase<TSelf>, new()
{
    public virtual IServiceCollection ConfigureOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}