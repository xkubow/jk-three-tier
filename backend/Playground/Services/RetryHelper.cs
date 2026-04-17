namespace JK.Playground.Services;

public static class RetryHelper
{
    public static async Task<T> Retry<T>(Func<CancellationToken, Task<T>> func, int maxRetries, TimeSpan delay, CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        if (maxRetries <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries));
        if (delay <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay));

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await func(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("Max retries exceeded without successful operation completion.");
    }
}