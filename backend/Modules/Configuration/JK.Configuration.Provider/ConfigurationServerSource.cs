using Microsoft.Extensions.Configuration;

namespace JK.Configuration.Provider;

public sealed class ConfigurationServerSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new ConfigurationServerProvider(builder);
}