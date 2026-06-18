using System.Reflection;
using JK.Platform.Core.DependencyInjection;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JK.Platform.Core.Test;

public class ServiceDiscoveryTests
{
    [Fact]
    public void RegisterInjectableServices_ShouldRegisterStandardInjectable()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.RegisterInjectableServices(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IStandardService>();
        Assert.NotNull(service);
        Assert.IsType<StandardService>(service);
    }

    [Fact]
    public void RegisterInjectableServices_ShouldRegisterMultipleInjectable()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.RegisterInjectableServices(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        
        var service1 = provider.GetService<IMultipleService1>();
        Assert.NotNull(service1);
        Assert.IsType<MultipleService>(service1);

        var service2 = provider.GetService<IMultipleService2>();
        Assert.NotNull(service2);
        Assert.IsType<MultipleService>(service2);
    }

    [Fact]
    public void RegisterInjectableServices_ShouldExcludeCommonInterfaceAndIDisposable()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.RegisterInjectableServices(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        
        // ICommonInterface should NOT be registered because it has [CommonInterface]
        var commonService = provider.GetService<ICommonInterface>();
        Assert.Null(commonService);

        // IDisposable should NOT be registered
        var disposableService = provider.GetService<IDisposable>();
        // Note: IDisposable might be registered by framework, but our MultipleInjectable should not add MultipleService as IDisposable
        var registrations = services.Where(s => s.ServiceType == typeof(IDisposable) && s.ImplementationType == typeof(MultipleService));
        Assert.Empty(registrations);
    }

    [Fact]
    public void RegisterInjectableServices_ShouldRespectOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.RegisterInjectableServices(assembly);

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(IOrderedService)).ToList();
        
        Assert.Equal(2, descriptors.Count);
        Assert.Equal(typeof(OrderedServiceLow), descriptors[0].ImplementationType);
        Assert.Equal(typeof(OrderedServiceHigh), descriptors[1].ImplementationType);
    }

    [Fact]
    public void RegisterInjectableServices_ShouldHandleGenericTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.RegisterInjectableServices(assembly);

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IGenericService<>));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(GenericService<>), descriptor.ImplementationType);
    }

    [Fact]
    public void RegisterInjectableServices_ShouldRespectLifetimes()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.RegisterInjectableServices(assembly);

        // Assert
        var singleton = services.First(s => s.ImplementationType == typeof(SingletonService));
        Assert.Equal(ServiceLifetime.Singleton, singleton.Lifetime);

        var scoped = services.First(s => s.ImplementationType == typeof(ScopedService));
        Assert.Equal(ServiceLifetime.Scoped, scoped.Lifetime);

        var transient = services.First(s => s.ImplementationType == typeof(TransientService));
        Assert.Equal(ServiceLifetime.Transient, transient.Lifetime);
    }

    [Fact]
    public void RegisterInjectableServices_ShouldPickBestInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.RegisterInjectableServices(assembly);

        // Assert
        // BestMatchingService implements IOtherInterface and IBestMatchingService.
        // It should pick IBestMatchingService.
        var descriptor = services.First(s => s.ImplementationType == typeof(BestMatchingService));
        Assert.Equal(typeof(IBestMatchingService), descriptor.ServiceType);
    }
}

// Test Classes and Interfaces

public interface IStandardService { }

[Injectable]
public class StandardService : IStandardService { }

public interface IMultipleService1 { }
public interface IMultipleService2 { }

[CommonInterface]
public interface ICommonInterface { }

[MultipleInjectable]
public class MultipleService : IMultipleService1, IMultipleService2, ICommonInterface, IDisposable
{
    public void Dispose() { }
}

public interface IOrderedService { }

[MultipleInjectable(Order = 1)]
public class OrderedServiceLow : IOrderedService { }

[MultipleInjectable(Order = 2)]
public class OrderedServiceHigh : IOrderedService { }

public interface IGenericService<T> { }

[Injectable]
public class GenericService<T> : IGenericService<T> { }

public interface ISingletonService { }
[Injectable(ServiceLifetime.Singleton)]
public class SingletonService : ISingletonService { }

public interface IScopedService { }
[Injectable(ServiceLifetime.Scoped)]
public class ScopedService : IScopedService { }

public interface ITransientService { }
[Injectable(ServiceLifetime.Transient)]
public class TransientService : ITransientService { }

public interface IOtherInterface { }
public interface IBestMatchingService { }
[Injectable]
public class BestMatchingService : IOtherInterface, IBestMatchingService { }
