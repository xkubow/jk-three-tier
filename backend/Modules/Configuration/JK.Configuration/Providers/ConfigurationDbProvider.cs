using System.Reflection;
using JK.Configuration.ConfigurationProviders;
using JK.Configuration.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JK.Configuration.Providers;

public sealed class ConfigurationDbProvider : ConfigurationProvider
{
    private readonly ConfigurationDbSource _source;
    private readonly IConfigurationRoot _bootstrapConfiguration;
    private readonly string? _marketCode;
    private readonly string _serviceCode;
    private readonly ILogger<ConfigurationDbProvider>? _logger;

    public ConfigurationDbProvider(
        IConfigurationBuilder configurationBuilder,
        ConfigurationDbSource source)
    {
        _source = source;

        var bootstrapBuilder = new ConfigurationBuilder();

        foreach (var registeredSource in configurationBuilder.Sources)
        {
            if (registeredSource is not ConfigurationDbSource)
            {
                bootstrapBuilder.Add(registeredSource);
            }
        }

        _bootstrapConfiguration = bootstrapBuilder.Build();

        _marketCode = _source.MarketCode ?? _bootstrapConfiguration["Platform:MarketCode"];

        _serviceCode =
            _source.ServiceCode
            ?? _bootstrapConfiguration["Platform:ServiceCode"]
            ?? Assembly.GetEntryAssembly()?.GetName().Name
            ?? "UnknownService";

        using var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddConfiguration(_bootstrapConfiguration.GetSection("Logging"));
            logging.AddConsole();
        });

        _logger = loggerFactory.CreateLogger<ConfigurationDbProvider>();
    }

    public override void Load()
    {
        try
        {
            _logger?.LogInformation(
                "Loading configuration from DB. MarketCode={MarketCode}, ServiceCode={ServiceCode}",
                _marketCode,
                _serviceCode);

            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();
            optionsBuilder.UseNpgsql(_source.ConnectionString);

            _source.ConfigureDbContext?.Invoke(optionsBuilder);

            using var context = new ConfigurationDbContext(optionsBuilder.Options);

            var rows = context.Configurations
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Where(x => x.MarketCode == null || x.MarketCode == _marketCode)
                .Where(x => x.ServiceCode == null || x.ServiceCode == _serviceCode)
                .ToList()
                .OrderBy(x => x.Key)
                .ThenBy(x => GetSpecificity(x.MarketCode, x.ServiceCode))
                .ThenBy(x => x.UpdatedAt);

            var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Key))
                {
                    continue;
                }

                data[row.Key] = row.Value;
            }

            Data = data;

            _logger?.LogInformation("Loaded {Count} configuration entries from DB.", Data.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load configuration from database.");

            if (!_source.Optional)
            {
                throw;
            }

            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static int GetSpecificity(string? marketCode, string? serviceCode)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(marketCode))
        {
            score += 1;
        }

        if (!string.IsNullOrWhiteSpace(serviceCode))
        {
            score += 2;
        }

        return score;
    }
}