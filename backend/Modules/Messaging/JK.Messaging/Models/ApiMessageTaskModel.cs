using JK.Platform.Persistence.EfCore;

namespace JK.Messaging.Models;

public sealed class ApiMessageTaskModel : ModelBase<string>
{
    public string TaskName { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public int MaxAttempts { get; set; } = 5;
    public TimeSpan? Delay { get; set; }
    public TimeSpan? RetryDelay { get; set; }
}