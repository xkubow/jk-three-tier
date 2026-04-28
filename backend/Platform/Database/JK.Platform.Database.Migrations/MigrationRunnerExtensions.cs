using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Database.Migrations;

public static class MigrationRunnerExtensions
{
    public static IServiceCollection AddBackendMigrations(
        this IServiceCollection services,
        string connectionString,
        params Assembly[] migrationAssemblies)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        if (migrationAssemblies.Length == 0)
            throw new ArgumentException("At least one migration assembly is required.", nameof(migrationAssemblies));

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddPostgres()
                    .WithGlobalConnectionString(connectionString);

                foreach (var assembly in migrationAssemblies)
                    rb.ScanIn(assembly).For.Migrations().For.EmbeddedResources();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}
