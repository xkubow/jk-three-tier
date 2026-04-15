using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.DependencyInjection;

public class ServiceRegistration
{
   public object? Key { get; set; }
   public Type InterfaceType { get; set; }
   public Type ImplementationType { get; set; }
   public int Priority { get; set; }
   public bool Multiple { get; set; }
   public ServiceLifetime Lifetime { get; set; }
   public int Order { get; set; }
}