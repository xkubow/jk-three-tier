using JK.Messaging.Contracts.Enums;
namespace JK.Messaging.States;

[GenerateSerializer]
public sealed class ApiMessageTaskState
{
    [Id(0)]
    public string TaskId { get; set; } = string.Empty;

    [Id(1)]
    public string TaskName { get; set; } = string.Empty;

    [Id(2)]
    public string TargetUrl { get; set; } = string.Empty;

    [Id(3)]
    public ApiMessageStateEnum TaskState { get; set; } = ApiMessageStateEnum.Waiting;

    [Id(4)]
    public int Attempts { get; set; }

    [Id(5)]
    public int MaxAttempts { get; set; } = 5;

    [Id(6)]
    public string? LastError { get; set; }

    [Id(7)]
    public DateTime CreatedOn { get; set; }

    [Id(8)]
    public DateTime? StartTime { get; set; }

    [Id(9)]
    public DateTime? FinishTime { get; set; }

    [Id(10)]
    public DateTime? NextRetryOn { get; set; }
}