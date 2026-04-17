using System.Reflection;
using JK.Platform.Core.Abstraction;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Core.AspNetCore.Hosting;

public abstract class ProgramBase<TStartup> where TStartup : class
{
    protected static IConfigurationRoot? Configuration { get; private set; }
    protected static ILogger<ProgramBase<TStartup>>? Logger { get; private set; }

    protected static readonly string ApplicationDescription =
        Assembly.GetEntryAssembly()?.GetName().Name
        ?? typeof(TStartup).Assembly.GetName().Name
        ?? typeof(TStartup).Name;

    protected static IEnumerable<IHostConfigurator> CreateHostConfigurators()
        => Array.Empty<IHostConfigurator>();

    protected static void ConfigureWebHost(string[] args)
        => ConfigureHost(args, hostBuilder =>
        {
            hostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseStartup<TStartup>();
            });
        });

    protected static void ConfigureHost(string[] args, Action<IHostBuilder>? hostBuilderAction = null)
    {
        var configurators = CreateHostConfigurators().ToArray();

        try
        {
            Configuration = CreateConfiguration(args, configurators);

            foreach (var configurator in configurators)
            {
                configurator.ApplicationStarting(Configuration);
            }

            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(builder =>
                {
                    foreach (var configurator in configurators)
                    {
                        builder = configurator.ConfigureHostConfiguration(builder);
                    }
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    foreach (var configurator in configurators)
                    {
                        builder = configurator.ConfigureAppConfiguration(hostContext, builder);
                    }
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    foreach (var configurator in configurators)
                    {
                        builder = configurator.ConfigureLogging(hostContext, builder);
                    }
                });

            hostBuilderAction?.Invoke(hostBuilder);

            foreach (var configurator in configurators)
            {
                configurator.ApplicationBeforeRun(Configuration);
            }

            var app = hostBuilder.Build();
            Logger = app.Services.GetService(typeof(ILogger<ProgramBase<TStartup>>)) as ILogger<ProgramBase<TStartup>>;

            app.Run();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "{ApplicationName} failed to start.", ApplicationDescription);

            foreach (var configurator in configurators)
            {
                configurator.ApplicationStoppedWithException(ex);
            }

            throw;
        }
        finally
        {
            foreach (var configurator in configurators)
            {
                configurator.ApplicationStopped();
            }
        }
    }

    protected static IConfigurationRoot CreateConfiguration(
        string[] args,
        IEnumerable<IHostConfigurator> configurators)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        // Allow configurators to participate if needed via ConfigureHostConfiguration
        var dummyHostContext = new HostBuilderContext(new Dictionary<object, object>());
        foreach (var configurator in configurators)
        {
            builder = configurator.ConfigureAppConfiguration(dummyHostContext, builder);
        }

        return builder.Build();
    }
}

