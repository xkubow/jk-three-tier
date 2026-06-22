using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Core.DistributedLock;
using Medallion.Threading.Redis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace JK.Platform.DistributedLock.Redis;

[Injectable(ServiceLifetime.Singleton)]
public class RedisDistributedLockService(IRedisClient redisClient) : IDistributedLock
{
    private readonly IRedisClient _redisClient = redisClient;

    public async Task<IDisposable?> TryAcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        var medallionLock = new RedisDistributedLock(resource, _redisClient.GetDefaultDatabase().Database);
        return await medallionLock.TryAcquireAsync(timeout, cancellationToken);
    }

    public async Task<IDisposable> AcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        var medallionLock = new RedisDistributedLock(resource, _redisClient.GetDefaultDatabase().Database);
        return await medallionLock.AcquireAsync(timeout, cancellationToken);
    }
}
