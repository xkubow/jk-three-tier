using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JK.Messaging.States;
using JK.Platform.Grpc.Client.Factory;
using NCrontab;

namespace JK.Messaging.Grains;

public sealed class ApiMessageGrain(
    [PersistentState("ApiMessage", "orleans")] IPersistentState<ApiMessageState> state,
    IGrpcGenericClientFactory genericClientFactory)
    : Grain, IApiMessageGrain, IRemindable
{
    private static (string ChannelUrl, string ServiceName, string MethodName) ParseGrpcUrl(string url)
    {
        var uri = new Uri(url);
        var scheme = uri.Scheme.ToLowerInvariant();
        if (scheme != "grpc" && scheme != "grpcs")
            throw new ArgumentException($"Unsupported scheme '{uri.Scheme}'. Use 'grpc' or 'grpcs'.");

        var channelScheme = scheme == "grpcs" ? "https" : "http";
        var channelUrl = $"{channelScheme}://{uri.Host}:{uri.Port}";

        var path = uri.AbsolutePath.Trim('/');
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            throw new ArgumentException($"Invalid gRPC URL path: '{uri.AbsolutePath}'. Expected 'serviceFullName/methodName'.");

        var serviceName = parts[0];
        var methodName = parts[1];
        return (channelUrl, serviceName, methodName);
    }

    public async Task SendApiMessage(string fullUrl)
    {
        Console.WriteLine($"Sending message to {fullUrl}");
        try
        {
            var (channelUrl, serviceName, methodName) = ParseGrpcUrl(fullUrl);
            var nativeClient = genericClientFactory.GetClient(channelUrl);

            // For demo/TestCall we send google.protobuf.Empty request
            var requestBytes = new Empty().ToByteArray();
            var responseBytes = await nativeClient.CallRawAsync(serviceName, methodName, requestBytes);
            Console.WriteLine($"Native gRPC call successful: {serviceName}/{methodName}, response bytes: {responseBytes?.Length ?? 0}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Native gRPC call failed: {ex.Message}");
        }
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (state.State.CronExpression is null) return;

        // Perform the task logic here
        await SendApiMessage("testUrl");
        state.State.TaskName = reminderName;
        await state.WriteStateAsync();
        
        // Reschedule for next occurrence
        var schedule = CrontabSchedule.Parse(state.State.CronExpression);
        var nextOccurrence = schedule.GetNextOccurrence(DateTime.UtcNow);
        var dueTime = nextOccurrence - DateTime.UtcNow;
        
        if (dueTime < TimeSpan.Zero) dueTime = TimeSpan.FromSeconds(1);
        
        await this.RegisterOrUpdateReminder(reminderName, dueTime, TimeSpan.FromMinutes(1));
    }

    public async Task<bool> Register(string cronString)
    {
        Console.WriteLine($"Registering with cron string: {cronString}");
        try
        {
            var schedule = CrontabSchedule.Parse(cronString);
            state.State.CronExpression = cronString;
            state.State.TaskName = this.GetPrimaryKeyString();
            await state.WriteStateAsync();

            var nextOccurrence = schedule.GetNextOccurrence(DateTime.UtcNow);
            var dueTime = nextOccurrence - DateTime.UtcNow;
            
            if (dueTime < TimeSpan.Zero) dueTime = TimeSpan.FromSeconds(1);

            await this.RegisterOrUpdateReminder(state.State.TaskName, dueTime, TimeSpan.FromMinutes(1));
            return true;
        }
        catch (CrontabException)
        {
            return false;
        }
    }
}