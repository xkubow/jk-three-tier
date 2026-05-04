namespace JK.Messaging.Grains;

public interface IApiMessageRecurringTaskSchedulerGrain: IGrainWithStringKey
{
    Task EnsureSyncReminderAsync();

    Task SyncRemindersAsync();

    Task ExecuteRecurringTaskAsync(string taskName);
}