namespace JK.Platform.LongRunningTasks.Options;

public class LongRunningTaskOptions
{
    public const string SectionName = "LongRunningTasks";

    public int PollIntervalSeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 50;

    public int MaxDegreeOfParallelism { get; set; } = 3;

    public int LockTimeoutMinutes { get; set; } = 10;

    public int RetryBaseDelaySeconds { get; set; } = 60;

    public int MaxRetryDelaySeconds { get; set; } = 900;
}
