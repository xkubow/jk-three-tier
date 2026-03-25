using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JK.Backend.Migrations;

public static class MigrationExecutionExtensions
{
    public static void RunBackendMigrations(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("JK.Backend.Migrations");

        logger.LogInformation("Applying database migrations.");

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

        logger.LogInformation("Database migrations completed.");
    }
}
