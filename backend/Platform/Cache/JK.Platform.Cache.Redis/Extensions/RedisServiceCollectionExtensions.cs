using JK.Platform.Cache.Redis.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace JK.Platform.Cache.Redis.Extensions;

public static class RedisServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheConfiguration = new CacheConfiguration();
        var cacheSection = configuration.GetSection("CacheConfiguration");
        cacheSection.Bind(cacheConfiguration);

        services.Configure<CacheConfiguration>(cacheSection);

        var redisConfiguration = new RedisConfiguration();
        configuration.GetSection(cacheConfiguration.SectionName).Bind(redisConfiguration);

        var serializer = new SystemTextJsonSerializer();
        services.AddSingleton<ISerializer>(serializer);

        var poolManager = new RedisConnectionPoolManager(redisConfiguration);
        services.AddSingleton<IRedisConnectionPoolManager>(poolManager);

        services.AddSingleton<IRedisClient>(new RedisClient(poolManager, serializer, redisConfiguration));

        return services;
    }
}