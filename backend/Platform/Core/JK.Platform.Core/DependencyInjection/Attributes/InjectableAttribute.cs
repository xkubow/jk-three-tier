using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.DependencyInjection.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class InjectableAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; }

    public InjectableAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        Lifetime = lifetime;
    }
}