using System.Reflection;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.DependencyInjection;

public static class ServiceDiscovery
{
    public static IServiceCollection RegisterInjectableServices(
        this IServiceCollection services,
        Assembly assembly)
    {
        var injectableTypes = SafeGetTypes(assembly)
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<InjectableAttribute>()
            })
            .Where(x => x.Attribute is not null)
            .ToList();

        foreach (var item in injectableTypes)
        {
            var serviceType = ResolveServiceType(item.Type);
            var lifetime = item.Attribute!.Lifetime;

            var descriptor = new ServiceDescriptor(serviceType, item.Type, lifetime);
            services.Add(descriptor);
        }

        return services;
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
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

    private static Type ResolveServiceType(Type implementationType)
    {
        var interfaces = implementationType
            .GetInterfaces()
            .Where(i => !IsFrameworkInterface(i))
            .ToArray();

        return interfaces.FirstOrDefault() ?? implementationType;
    }

    private static bool IsFrameworkInterface(Type type)
    {
        var ns = type.Namespace ?? string.Empty;
        return ns.StartsWith("System", StringComparison.Ordinal)
               || ns.StartsWith("Microsoft", StringComparison.Ordinal);
    }
}