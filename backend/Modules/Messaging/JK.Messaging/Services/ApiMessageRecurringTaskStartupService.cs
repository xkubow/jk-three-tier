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

        await grain.EnsureSyncReminderAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}