using System.Diagnostics.Metrics;

namespace JK.Platform.LongRunningTasks.Observability;

public sealed class LongRunningTaskMetrics
{
    public const string MeterName = "JK.Platform.LongRunningTasks";

    private readonly Counter<long> _tasksPending;
    private readonly Counter<long> _tasksRunning;
    private readonly Counter<long> _tasksFailed;
    private readonly Counter<long> _itemsProcessed;
    private readonly Histogram<double> _taskDurationSeconds;

    public LongRunningTaskMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _tasksPending = meter.CreateCounter<long>("tasks_pending");
        _tasksRunning = meter.CreateCounter<long>("tasks_running");
        _tasksFailed = meter.CreateCounter<long>("tasks_failed");
        _itemsProcessed = meter.CreateCounter<long>("items_processed_total");
        _taskDurationSeconds = meter.CreateHistogram<double>("task_duration_seconds");
    }

    public void RecordTaskClaimed(string taskName) =>
        _tasksPending.Add(1, new KeyValuePair<string, object?>("task_name", taskName));

    public void RecordTaskRunning(string taskName) =>
        _tasksRunning.Add(1, new KeyValuePair<string, object?>("task_name", taskName));

    public void RecordTaskFailed(string taskName) =>
        _tasksFailed.Add(1, new KeyValuePair<string, object?>("task_name", taskName));

    public void RecordItemsProcessed(string taskName, long count) =>
        _itemsProcessed.Add(count, new KeyValuePair<string, object?>("task_name", taskName));

    public void RecordTaskDuration(string taskName, double durationSeconds) =>
        _taskDurationSeconds.Record(durationSeconds, new KeyValuePair<string, object?>("task_name", taskName));
}
