using System.Collections.Concurrent;
using System.Net.Mime;

namespace JK.Playground.Stores;

public class IdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyEntry> _idempotencyCache = new();
    private readonly TimeSpan _expirationTime = new TimeSpan(0, 10, 0);

    public IdempotencyEntry? Get(string key) => _idempotencyCache!.GetValueOrDefault(key, null);

    public IdempotencyEntry GetOrAdd(string key, string responseBody)
    {
        IdempotencyEntry entry = new() { ExpiresAt = DateTime.UtcNow.Add(_expirationTime), ResponseBody = responseBody, Lock = false };
        entry = _idempotencyCache.GetOrAdd(key, _ => entry);
        return entry;
    }

    public bool TrySetLock(string key, bool lockState)
    {
        if(!_idempotencyCache.ContainsKey(key))
            return false;
        _idempotencyCache[key].Lock = lockState;
        return true;
    }

    public bool TryRemove(string key)
    {
        return _idempotencyCache.TryRemove(key, out _);
    }

    public void Add(string key, string responseContent, int statusCode, string? contentType = MediaTypeNames.Application.Json)
    {
        _idempotencyCache[key] = new IdempotencyEntry
        {
            ResponseBody = responseContent,
            ExpiresAt = DateTime.UtcNow.Add(_expirationTime),
            Lock = false,
            StatusCode = statusCode,
            ContentType = contentType
        };
    }
}

public class IdempotencyEntry
{
    public DateTime ExpiresAt { get; set; }
    public int StatusCode { get; set; }
    public string? ContentType { get; set; }
    public string ResponseBody { get; set; }
    public bool Lock { get; set; }
}