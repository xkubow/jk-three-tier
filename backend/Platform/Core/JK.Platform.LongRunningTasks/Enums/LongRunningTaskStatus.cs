namespace JK.Platform.LongRunningTasks.Enums;

public enum LongRunningTaskStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Retrying,
    Cancelled,
    PartiallyCompleted
}
