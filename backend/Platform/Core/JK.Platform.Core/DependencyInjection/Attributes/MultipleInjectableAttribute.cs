using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.DependencyInjection.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MultipleInjectableAttribute : Attribute
{
   public ServiceLifetime Lifetime { get; set; }
   public int Order { get; set; }

   public MultipleInjectableAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient, int order = 1000000)
   {
      Lifetime = lifetime;
      Order = order;
   }

   public IEnumerable<ServiceRegistration> ToServiceRegistration(Type targetType)
   {
      var interfaceTypes = targetType.GetInterfaces().Where(x => x.FullName != "System.IDisposable" && !x.GetCustomAttributes(typeof(CommonInterfaceAttribute), false).Any()).ToArray();

      foreach (var interfaceType in interfaceTypes)
      {
         yield return new ServiceRegistration()
         {
            ImplementationType = targetType,
            InterfaceType = interfaceType,
            Lifetime = Lifetime,
            Multiple = true,
            Order = Order
         };
      }
   }
}