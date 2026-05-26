using JK.Platform.LongRunningTasks.Options;

namespace JK.Platform.LongRunningTasks.Retry;

public static class LongRunningTaskRetryPolicy
{
    public static DateTime GetNextRunAtUtc(int attemptCount, LongRunningTaskOptions? options = null)
    {
        options ??= new LongRunningTaskOptions();

        var baseDelay = TimeSpan.FromSeconds(Math.Max(1, options.RetryBaseDelaySeconds));
        var maxDelay = TimeSpan.FromSeconds(Math.Max(options.RetryBaseDelaySeconds, options.MaxRetryDelaySeconds));

        var exponent = Math.Clamp(attemptCount - 1, 0, 10);
        var delay = TimeSpan.FromTicks(baseDelay.Ticks * (long)Math.Pow(2, exponent));

        if (delay > maxDelay)
            delay = maxDelay;

        return DateTime.UtcNow.Add(delay);
    }
}
