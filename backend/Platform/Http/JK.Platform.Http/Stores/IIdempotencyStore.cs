namespace JK.Platform.Http.Stores;

public interface IIdempotencyStore
{
    IdempotencyEntry? Get(string key);
    IdempotencyEntry GetOrAdd(string key, string responseBody);
    bool TrySetLock(string key, bool lockState);
    bool TryRemove(string key);
    void Add(string key, string responseContent, int statusCode, string contentType = "application/json");
}