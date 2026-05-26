using JK.Platform.LongRunningTasks.Entities;

namespace JK.Platform.LongRunningTasks.Abstractions;

public interface ILongRunningTaskHandler
{
    string TaskName { get; }

    Task ExecuteAsync(
        LongRunningTaskEntity task,
        CancellationToken cancellationToken);
}
