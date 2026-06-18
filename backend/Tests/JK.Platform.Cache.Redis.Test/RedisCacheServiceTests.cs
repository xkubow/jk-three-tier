using JK.Platform.Cache.Redis.Configurations;
using JK.Platform.Core.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.System.Text.Json;
using Xunit;

namespace JK.Platform.Cache.Redis.Test;

public class RedisCacheServiceTests
{
    private readonly IServiceProvider _serviceProvider;

    public RedisCacheServiceTests()
    {
        var services = new ServiceCollection();
        
        var redisConfiguration = new RedisConfiguration()
        {
            Hosts = new[]
            {
                new RedisHost { Host = "localhost", Port = 6379 }
            }
        };

        var serializer = new SystemTextJsonSerializer();
        var poolManager = new RedisConnectionPoolManager(redisConfiguration);
        services.AddSingleton<IRedisConnectionPoolManager>(poolManager);
        services.AddSingleton<ISerializer>(serializer);
        services.AddSingleton<IRedisClient>(new RedisClient(poolManager, serializer, redisConfiguration));
        
        services.Configure<CacheConfiguration>(options =>
        {
            options.DefaultExpirationSecond = 60;
        });

        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<ICacheService, RedisCacheService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var cacheService = _serviceProvider.GetRequiredService<ICacheService>();
        var prefix = "test-prefix";
        var key = Guid.NewGuid().ToString();

        // Act
        var result = await cacheService.GetAsync<string>(prefix, key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_ShouldWork()
    {
        // Arrange
        var cacheService = _serviceProvider.GetRequiredService<ICacheService>();
        var key = Guid.NewGuid().ToString();
        var value = "test-value-" + key;
        var prefix = "test-prefix";

        // Act
        await cacheService.SetAsync(prefix, key, value);
        var result = await cacheService.GetAsync<string>(prefix, key);

        // Assert
        Assert.Equal(value, result);
        
        // Cleanup
        await cacheService.RemoveAsync(prefix, key);
        result = await cacheService.GetAsync<string>(prefix, key);
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveKey()
    {
        // Arrange
        var cacheService = _serviceProvider.GetRequiredService<ICacheService>();
        var key = Guid.NewGuid().ToString();
        var value = new TestData { Id = 1, Name = "Test" };
        var prefix = "test-data";

        // Act
        await cacheService.SetAsync(prefix, key, value);
        await cacheService.RemoveAsync(prefix, key);
        var result = await cacheService.GetAsync<TestData>(prefix, key);

        // Assert
        Assert.Null(result);
    }

    public class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
