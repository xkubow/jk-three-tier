using JK.Messaging.Grains;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace JK.Messaging.Tasks;

public class ApiMessageRecurringTaskStartupTask : IStartupTask
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<ApiMessageRecurringTaskStartupTask> _logger;

    public ApiMessageRecurringTaskStartupTask(
        IGrainFactory grainFactory,
        ILogger<ApiMessageRecurringTaskStartupTask> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var grain = _grainFactory.GetGrain<IApiMessageRecurringTaskSchedulerGrain>("api-message-recurring-scheduler");

        const int maxRetries = 10;
        int retryCount = 0;

        while (true)
        {
            try
            {
                await grain.EnsureSyncReminderAsync();
                _logger.LogInformation("Successfully initialized recurring task scheduler.");
                break;
            }
            catch (Exception ex) when (IsRetryable(ex) && retryCount < maxRetries)
            {
                retryCount++;
                _logger.LogWarning(
                    ex,
                    "Failed to initialize recurring task scheduler (Attempt {RetryCount}/{MaxRetries}). Retrying in 2 seconds...",
                    retryCount,
                    maxRetries);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Failed to initialize recurring task scheduler after {RetryCount} retries. The silo may be in an unstable state.",
                    retryCount);

                throw;
            }
        }
    }

    private static bool IsRetryable(Exception ex)
    {
        if (ex is OrleansMessageRejectionException or TimeoutException or TaskCanceledException)
        {
            return true;
        }

        if (ex.InnerException != null)
        {
            return IsRetryable(ex.InnerException);
        }

        return false;
    }
}