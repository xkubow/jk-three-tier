namespace JK.Messaging.States;

[GenerateSerializer, Alias(nameof(ApiMessageState))]
public class ApiMessageState
{
    [Id(0)] public string TaskName { get; set; } = null!;
    [Id(1)] public string? CronExpression { get; set; }
}