using JK.Messaging.Contracts.Enums;

namespace JK.Messaging.Contracts;

public class ApiMessageTaskDto
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public ApiMessageStateEnum TaskState { get; set; }
    public int Attempts { get; set; }
    public int MaxAttempts { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    public DateTime? NextRetryOn { get; set; }
}
