using FluentAssertions;
using JK.Platform.Cache.Redis.Extensions;
using JK.Platform.Core.DependencyInjection;
using JK.Platform.Core.DistributedLock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace JK.Platform.DistributedLock.Redis.Test;

public class RedisDistributedLockServiceTests
{
    private readonly IServiceProvider _serviceProvider;

    public RedisDistributedLockServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"CacheConfiguration:Provider", "Redis"},
            {"CacheConfiguration:SectionName", "MyRedisSettings"},
            {"MyRedisSettings:AllowAdmin", "true"},
            {"MyRedisSettings:ConnectTimeout", "5000"},
            {"MyRedisSettings:Database", "0"},
            {"MyRedisSettings:Hosts:0:Host", "localhost"}, 
            {"MyRedisSettings:Hosts:0:Port", "6379"},
            {"MyRedisSettings:PoolSize", "2"}
        };

        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(inMemorySettings!);

        // Register Redis Client
        builder.Services.AddRedisCache(builder.Configuration);
        
        // Register DistributedLock Service
        builder.Services.RegisterInjectableServices(typeof(RedisDistributedLockService).Assembly);
        
        _serviceProvider = builder.Services.BuildServiceProvider();
    }

    [Fact]
    public void ServiceShouldBeRegistered()
    {
        var distributedLock = _serviceProvider.GetService<IDistributedLock>();
        
        distributedLock.Should().NotBeNull();
        distributedLock.Should().BeOfType<RedisDistributedLockService>();
    }

    [Fact]
    public async Task TryAcquireAsyncShouldWorkWithRealRedis()
    {
        var service = _serviceProvider.GetRequiredService<IDistributedLock>();
        string resource = $"test-lock-{Guid.NewGuid()}";
        
        // Act
        using var lockHandle = await service.TryAcquireAsync(resource, TimeSpan.FromSeconds(5));

        // Assert
        lockHandle.Should().NotBeNull();
    }

    [Fact]
    public async Task AcquireAsyncShouldWorkWithRealRedis()
    {
        var service = _serviceProvider.GetRequiredService<IDistributedLock>();
        string resource = $"test-lock-{Guid.NewGuid()}";
        
        // Act
        using var lockHandle = await service.AcquireAsync(resource, TimeSpan.FromSeconds(5));

        // Assert
        lockHandle.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldNotBeAbleToAcquireLockedResource()
    {
        var service = _serviceProvider.GetRequiredService<IDistributedLock>();
        string resource = $"test-lock-{Guid.NewGuid()}";
        
        // Act
        using var lockHandle1 = await service.AcquireAsync(resource, TimeSpan.FromSeconds(5));
        var lockHandle2 = await service.TryAcquireAsync(resource, TimeSpan.FromMilliseconds(100));

        // Assert
        lockHandle2.Should().BeNull();
    }

    [Fact]
    public async Task AcquireAsyncShouldRespectCancellationToken()
    {
        var service = _serviceProvider.GetRequiredService<IDistributedLock>();
        string resource = $"test-lock-{Guid.NewGuid()}";
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        
        // Act & Assert
        var act = () => service.AcquireAsync(resource, TimeSpan.FromSeconds(5), cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryAcquireAsyncShouldRespectCancellationToken()
    {
        var service = _serviceProvider.GetRequiredService<IDistributedLock>();
        string resource = $"test-lock-{Guid.NewGuid()}";
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        
        // Act & Assert
        var act = () => service.TryAcquireAsync(resource, TimeSpan.FromSeconds(5), cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
