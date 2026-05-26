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

        var assembliesToScan = DiscoverAssemblies(migrationAssemblies);

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddPostgres()
                    .WithGlobalConnectionString(connectionString);

                foreach (var assembly in assembliesToScan)
                    rb.ScanIn(assembly).For.Migrations().For.EmbeddedResources();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }

    private static IEnumerable<Assembly> DiscoverAssemblies(IEnumerable<Assembly> assemblies)
    {
        var discovered = new HashSet<Assembly>();
        var queue = new Queue<Assembly>(assemblies);

        while (queue.Count > 0)
        {
            var assembly = queue.Dequeue();
            if (!discovered.Add(assembly)) continue;

            // Discover from attributes
            var assemblyAttributes = assembly.GetCustomAttributes<MigrationDependencyAttribute>();
            foreach (var attr in assemblyAttributes)
            {
                queue.Enqueue(attr.MarkerType.Assembly);
            }

            var types = GetLoadableTypes(assembly).ToList();
            var typeAttributes = types.SelectMany(t => t.GetCustomAttributes<MigrationDependencyAttribute>());

            foreach (var attr in typeAttributes)
            {
                queue.Enqueue(attr.MarkerType.Assembly);
            }

            // Discover from IMigrateWith<T> interfaces (covers PlatformMigrator<T> inheritance)
            var interfaceDependencies = types
                .SelectMany(t => t.GetInterfaces())
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMigrateWith<>));

            foreach (var itf in interfaceDependencies)
            {
                var dependentType = itf.GetGenericArguments()[0];
                queue.Enqueue(dependentType.Assembly);
            }
        }

        return discovered;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
