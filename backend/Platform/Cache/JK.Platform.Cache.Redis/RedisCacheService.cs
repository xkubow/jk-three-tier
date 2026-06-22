using JK.Platform.Cache.Redis.Configurations;
using JK.Platform.Core.Cache;
using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Core.Validations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace JK.Platform.Cache.Redis;

[Injectable(ServiceLifetime.Singleton)]
public class RedisCacheService(IRedisClient redisClient, ILogger<RedisCacheService> logger, IOptions<CacheConfiguration> options) : ICacheService
{
    private readonly IRedisClient _redisClient = redisClient;
    private readonly ILogger<RedisCacheService> _logger = logger;
    private readonly IOptions<CacheConfiguration> _options = options;

    public async Task<T?> GetAsync<T>(string keyPrefix, string key, CancellationToken cancellationToken = default)
    {
        Guard.NotNullAndNotEmpty(key, nameof(key));

        var value = await _redisClient.GetDefaultDatabase().GetAsync<T>(GetRedisKey(keyPrefix, key));

        return value;
    }

    public async Task SetAsync<T>(string keyPrefix, string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNullAndNotEmpty(key, nameof(key));

        var expiry = expiration ?? TimeSpan.FromSeconds(_options.Value.DefaultExpirationSecond);
        await _redisClient.GetDefaultDatabase().AddAsync(GetRedisKey(keyPrefix, key), value, expiry);
    }

    public async Task RemoveAsync(string keyPrefix, string key, CancellationToken cancellationToken = default)
    {
        Guard.NotNullAndNotEmpty(key, nameof(key));

        await _redisClient.GetDefaultDatabase().RemoveAsync(GetRedisKey(keyPrefix, key));
    }

    private static string GetRedisKey(string keyPrefix, string key) => $"{keyPrefix}:{key}";
}