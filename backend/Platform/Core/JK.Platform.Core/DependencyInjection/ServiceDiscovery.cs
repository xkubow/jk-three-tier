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
            .Select(t => new { Type = t, Attribute = t.GetCustomAttribute<InjectableAttribute>() })
            .Where(x => x.Attribute is not null)
            .ToList();

        foreach (var item in injectableTypes)
        {
            var implementationType = NormalizeImplementationType(item.Type);
            var serviceType = ResolveServiceType(item.Type);
            var lifetime = item.Attribute!.Lifetime;

            services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
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

    private static Type NormalizeImplementationType(Type implementationType)
    {
        return implementationType.IsGenericType
            ? implementationType.GetGenericTypeDefinition()
            : implementationType;
    }

    private static Type ResolveServiceType(Type implementationType)
    {
        var interfaces = implementationType
            .GetInterfaces()
            .Where(i => !IsFrameworkInterface(i))
            .ToArray();

        if (interfaces.Length == 0)
        {
            return NormalizeServiceType(implementationType);
        }

        var normalizedImplementationType = NormalizeImplementationType(implementationType);
        var implementationName = normalizedImplementationType.Name;

        if (normalizedImplementationType.IsGenericTypeDefinition)
        {
            implementationName = RemoveGenericArity(implementationName);
        }

        var matchingInterface = interfaces.FirstOrDefault(i =>
        {
            var interfaceType = NormalizeServiceType(i);
            var interfaceName = interfaceType.Name;

            if (interfaceType.IsGenericTypeDefinition)
            {
                interfaceName = RemoveGenericArity(interfaceName);
            }

            return interfaceName.Equals($"I{implementationName}", StringComparison.OrdinalIgnoreCase)
                   && (i.Namespace == implementationType.Namespace || i.Namespace == implementationType.Namespace + ".Abstractions");

        }) ?? interfaces.FirstOrDefault(i =>
        {
            var interfaceType = NormalizeServiceType(i);
            var interfaceName = interfaceType.Name;

            if (interfaceType.IsGenericTypeDefinition)
            {
                interfaceName = RemoveGenericArity(interfaceName);
            }

            return interfaceName.Equals($"I{implementationName}", StringComparison.OrdinalIgnoreCase);
        });

        return NormalizeServiceType(matchingInterface ?? interfaces.First());
    }

    private static Type NormalizeServiceType(Type serviceType)
    {
        if (serviceType.IsGenericType)
        {
            return serviceType.GetGenericTypeDefinition();
        }

        return serviceType;
    }

    private static string RemoveGenericArity(string name)
    {
        var index = name.IndexOf('`');
        return index >= 0 ? name[..index] : name;
    }

    private static bool IsFrameworkInterface(Type type)
    {
        var ns = type.Namespace ?? string.Empty;
        return ns.StartsWith("System", StringComparison.Ordinal)
               || ns.StartsWith("Microsoft", StringComparison.Ordinal);
    }
}