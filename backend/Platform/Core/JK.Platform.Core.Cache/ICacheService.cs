namespace JK.Platform.Core.Cache;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string keyPrefix, string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string keyPrefix, string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string keyPrefix, string key, CancellationToken cancellationToken = default);
}