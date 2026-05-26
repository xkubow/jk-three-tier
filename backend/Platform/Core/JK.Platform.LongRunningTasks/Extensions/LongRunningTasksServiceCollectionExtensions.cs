using System.Reflection;
using JK.Platform.LongRunningTasks.Abstractions;
using JK.Platform.LongRunningTasks.Observability;
using JK.Platform.LongRunningTasks.Options;
using JK.Platform.LongRunningTasks.Repositories;
using JK.Platform.LongRunningTasks.Services;
using JK.Platform.LongRunningTasks.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.LongRunningTasks.Extensions;

public static class LongRunningTasksServiceCollectionExtensions
{
    public static IServiceCollection AddLongRunningTasks<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] handlerAssemblies)
        where TContext : DbContext, ILongRunningTasksDbContext
    {
        services.Configure<LongRunningTaskOptions>(
            configuration.GetSection(LongRunningTaskOptions.SectionName));

        services.AddScoped<ILongRunningTaskRepository, LongRunningTaskRepository<TContext>>();
        services.AddScoped<ILongRunningTaskService, LongRunningTaskService>();
        services.AddScoped<LongRunningTaskHandlerRegistry>();
        services.AddSingleton<LongRunningTaskMetrics>();

        RegisterHandlers(services, handlerAssemblies);

        services.AddHostedService<LongRunningTaskWorker>();

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] handlerAssemblies)
    {
        var handlerTypes = handlerAssemblies
            .SelectMany(GetLoadableTypes)
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => typeof(ILongRunningTaskHandler).IsAssignableFrom(t))
            .Distinct()
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(typeof(ILongRunningTaskHandler), handlerType);
        }
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
