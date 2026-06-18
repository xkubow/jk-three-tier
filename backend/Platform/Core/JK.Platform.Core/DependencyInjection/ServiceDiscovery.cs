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
        var types = SafeGetTypes(assembly)
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        var registrations = new List<ServiceRegistration>();

        foreach (var type in types)
        {
            var injectableAttribute = type.GetCustomAttribute<InjectableAttribute>();
            if (injectableAttribute != null)
            {
                registrations.Add(new ServiceRegistration
                {
                    ImplementationType = type,
                    InterfaceType = ResolveServiceType(type),
                    Lifetime = injectableAttribute.Lifetime,
                    Order = 0
                });
            }

            var multipleInjectableAttribute = type.GetCustomAttribute<MultipleInjectableAttribute>();
            if (multipleInjectableAttribute != null)
            {
                registrations.AddRange(multipleInjectableAttribute.ToServiceRegistration(type));
            }
        }

        foreach (var reg in registrations.OrderBy(x => x.Order))
        {
            var implementationType = NormalizeImplementationType(reg.ImplementationType);
            var serviceType = NormalizeServiceType(reg.InterfaceType);

            services.Add(new ServiceDescriptor(serviceType, implementationType, reg.Lifetime));
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