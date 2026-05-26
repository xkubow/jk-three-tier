namespace JK.Platform.LongRunningTasks.Workers;

internal static class WorkerIdentity
{
    public static string GetWorkerId()
    {
        return Environment.GetEnvironmentVariable("HOSTNAME")
               ?? Environment.GetEnvironmentVariable("POD_NAME")
               ?? Environment.MachineName;
    }
}
