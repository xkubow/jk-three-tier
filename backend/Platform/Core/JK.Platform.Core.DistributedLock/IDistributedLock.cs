namespace JK.Platform.Core.DistributedLock;

public interface IDistributedLock
{
    Task<IDisposable?> TryAcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default);
    Task<IDisposable> AcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default);
}