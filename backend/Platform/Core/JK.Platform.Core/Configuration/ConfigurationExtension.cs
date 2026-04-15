using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JK.Platform.Core.Configuration;

public static class ConfigurationExtension
{
    private static readonly IDictionary<Type, IAppConfiguration> _configurations = new Dictionary<Type, IAppConfiguration>();

    public static IServiceCollection ConfigureConfiguration<T>(this IServiceCollection serviceCollection, IConfiguration configuration) where T : class, IAppConfiguration, new()
    {
        var section = configuration.GetConfigurationSection<T>();
        serviceCollection.Configure<T>(section);
        return serviceCollection;
    }

    public static T GetConfiguration<T>(this IConfiguration configuration) where T : IAppConfiguration, new()
    {
        var type = typeof(T);
        if (!_configurations.ContainsKey(type))
            _configurations.Add(type, new T());
        return configuration.GetSection(_configurations[type].SectionName).Get<T>() ?? new T();
    }

    public static string Market(this IConfiguration configuration) => configuration.GetRequired(ServerConfigurationKeys.Market);

    public static string? MarketArea(this IConfiguration configuration) => configuration.GetOptional(ServerConfigurationKeys.MarketArea);

    public static IConfigurationSection GetConfigurationSection<T>(this IConfiguration configuration) where T : IAppConfiguration, new()
    {
        var type = typeof(T);
        if (!_configurations.ContainsKey(type))
            _configurations.Add(type, new T());
        return configuration.GetSection(_configurations[type].SectionName);
    }

    public static string GetRequired(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (value is null)
            throw new ConfigurationKeyNotFoundException(key);
        return value;
    }

    public static string? GetOptional(this IConfiguration configuration, string key) => configuration[key];

    public static bool GetAsBool(this IConfiguration configuration, string key, bool defaultValue)
    {
        var value = configuration.GetOptional(key);
        if (value is null)
            return defaultValue;
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public static IConfigurationBuilder AddHostConfiguration(this IConfigurationBuilder configurationBuilder, string[] args)
    {
        var assemblyConfiguration = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Unknown";
        return configurationBuilder
            .SetBasePath(AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("hostsettings.json", true, true)
            .AddJsonFile($"hostsettings.{assemblyConfiguration}.json", true, true)
            .AddEnvironmentVariables(prefix: "Jp.")
            .AddCommandLine(args);
    }

    public static IConfigurationBuilder AddAppConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        var assemblyConfiguration = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Unknown";

        return configurationBuilder
            .SetBasePath(AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{assemblyConfiguration}.json", true, true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true);
    }

    public static int? GetOptionalAsInt(this IConfiguration configuration, string key)
    {
        var value = configuration.GetOptional(key);
        if (value is null)
            return null;
        return int.TryParse(value, out var result) ? result : null;
    }

    public static int GetAsInt(this IConfiguration configuration, string key, int defaultValue)
    {
        var value = configuration.GetOptional(key);
        if (value is null)
            return defaultValue;
        return int.TryParse(value, out var result) ? result : defaultValue;
    }
}