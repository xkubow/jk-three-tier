using System.Collections;
using System.Reflection;
using JK.Platform.Core.Abstraction;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Core.AspNetCore.Discovery;

public static class DomainDiscovery
{
    private const string AssemblyPrefix = "JK.";

    public static IReadOnlyList<Type> FindModuleInstallerTypes(ILogger? logger = null)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Where(a => a.GetName().Name?.StartsWith(AssemblyPrefix, StringComparison.Ordinal) == true)
            .SelectMany(a => SafeGetTypes(a, logger))
            .Where(IsConcreteInstallerType)
            .Distinct()
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<IModuleInstaller> CreateModuleInstallers(
        IEnumerable<Type> installerTypes,
        ILogger? logger = null)
    {
        var installers = new List<IModuleInstaller>();

        foreach (var installerType in installerTypes)
        {
            try
            {
                if (Activator.CreateInstance(installerType) is IModuleInstaller installer)
                {
                    installers.Add(installer);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to instantiate installer {InstallerType}.", installerType.FullName);
            }
        }

        return installers
            .OrderBy(x => x.ModuleName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly, ILogger? logger)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            logger?.LogWarning(ex, "Failed to fully load types from {AssemblyName}.", assembly.GetName().Name);
            return ex.Types.Where(x => x is not null)!;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to enumerate types from {AssemblyName}.", assembly.GetName().Name);
            return Array.Empty<Type>();
        }
    }

    private static bool IsConcreteInstallerType(Type type)
    {
        return typeof(IModuleInstaller).IsAssignableFrom(type)
               && !type.IsInterface
               && !type.IsAbstract
               && type.GetConstructor(Type.EmptyTypes) is not null;
    }

    public static IReadOnlyList<Assembly> FindDomainAssemblies(ILogger? logger = null)
    {
        // This returns all loaded assemblies starting with "JK."
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Where(a => a.GetName().Name?.StartsWith(AssemblyPrefix, StringComparison.Ordinal) == true)
            .ToList();
    }
}