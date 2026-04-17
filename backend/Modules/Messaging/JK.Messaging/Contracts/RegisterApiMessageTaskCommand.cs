namespace JK.Messaging.Contracts;

[GenerateSerializer]
public class RegisterApiMessageTaskCommand
{
    [Id(0)] public string Id { get; set; } = default!;
    [Id(1)] public string TaskName { get; set; } = string.Empty;
    [Id(2)] public string TargetUrl { get; set; } = string.Empty;
    [Id(3)] public int MaxAttempts { get; set; } = 5;
    [Id(4)] public TimeSpan? Delay { get; set; }
    [Id(5)] public TimeSpan? RetryDelay { get; set; }
}