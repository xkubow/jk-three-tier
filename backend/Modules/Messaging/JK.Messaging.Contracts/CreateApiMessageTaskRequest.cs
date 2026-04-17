namespace JK.Messaging.Contracts;

public sealed class CreateApiMessageTaskRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public int MaxAttempts { get; set; } = 5;
    public TimeSpan? Delay { get; set; }
    public TimeSpan? RetryDelay { get; set; }
}