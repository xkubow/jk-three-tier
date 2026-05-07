using JK.Messaging.Grains;
using Microsoft.Extensions.Hosting;

namespace JK.Messaging.Services;

public class ApiMessageRecurringTaskStartupService: IHostedService
{
    private readonly IGrainFactory _grainFactory;

    public ApiMessageRecurringTaskStartupService(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var grain = _grainFactory.GetGrain<IApiMessageRecurringTaskSchedulerGrain>(
            "api-message-recurring-scheduler");

        int retryCount = 0;
        const int maxRetries = 5;

        while (true)
        {
            try
            {
                await grain.EnsureSyncReminderAsync();
                break;
            }
            catch (OrleansMessageRejectionException ex) when (retryCount < maxRetries)
            {
                retryCount++;
                // Wait a bit for the cluster membership to stabilize
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}