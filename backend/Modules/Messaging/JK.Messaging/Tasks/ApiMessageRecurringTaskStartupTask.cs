using JK.Messaging.Grains;

namespace JK.Messaging.Tasks;

public class ApiMessageRecurringTaskStartupTask : IStartupTask
{
    private readonly IGrainFactory _grainFactory;
    public ApiMessageRecurringTaskStartupTask(IGrainFactory grainFactory) => _grainFactory = grainFactory;

    public async Task Execute(CancellationToken cancellationToken)
    {
        var grain = _grainFactory.GetGrain<IApiMessageRecurringTaskSchedulerGrain>("api-message-recurring-scheduler");
        await grain.EnsureSyncReminderAsync();
    }
}