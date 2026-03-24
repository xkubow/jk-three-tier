using JK.Configuration.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JK.Configuration.ConfigurationProviders;

public sealed class ConfigurationDbSource : IConfigurationSource
{
    public required string ConnectionString { get; init; }
    public string? MarketCode { get; init; }
    public string? ServiceCode { get; init; }
    public bool Optional { get; init; }
    public Action<DbContextOptionsBuilder>? ConfigureDbContext { get; init; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new ConfigurationDbProvider(builder, this);
}