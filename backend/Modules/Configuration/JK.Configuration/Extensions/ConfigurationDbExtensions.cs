using JK.Configuration.ConfigurationProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JK.Configuration.Extensions;

public static class ConfigurationDbExtensions
{
    public static IConfigurationBuilder AddConfigurationDatabase(
        this IConfigurationBuilder builder,
        Action<ConfigurationDbOptions> configure)
    {
        var options = new ConfigurationDbOptions();
        configure(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("Configuration DB connection string must be provided.");
        }

        builder.Add(new ConfigurationDbSource
        {
            ConnectionString = options.ConnectionString!,
            MarketCode = options.MarketCode,
            ServiceCode = options.ServiceCode,
            Optional = options.Optional,
            ConfigureDbContext = options.ConfigureDbContext
        });

        return builder;
    }
}

public sealed class ConfigurationDbOptions
{
    public string? ConnectionString { get; set; }
    public string? MarketCode { get; set; }
    public string? ServiceCode { get; set; }
    public bool Optional { get; set; }
    public Action<DbContextOptionsBuilder>? ConfigureDbContext { get; set; }
}