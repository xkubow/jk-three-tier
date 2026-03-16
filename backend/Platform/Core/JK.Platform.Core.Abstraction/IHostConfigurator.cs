using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Core.Abstraction;

public interface IHostConfigurator
{
    void ApplicationStarting(IConfiguration configuration);

    IConfigurationBuilder ConfigureHostConfiguration(IConfigurationBuilder builder);

    IConfigurationBuilder ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder builder);

    ILoggingBuilder ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder builder);

    void ApplicationBeforeRun(IConfiguration configuration);

    void ApplicationStopped();

    void ApplicationStoppedWithException(Exception exception);
}
