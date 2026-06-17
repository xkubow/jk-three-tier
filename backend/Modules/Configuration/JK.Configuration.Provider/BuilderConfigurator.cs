using JK.Platform.Core.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JK.Configuration.Provider;

public class BuilderConfigurator: BuilderConfiguratorBase
{
    public override void AddConfiguration(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{applicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        applicationBuilder.AddConfigurationServerProvider();
    }
}