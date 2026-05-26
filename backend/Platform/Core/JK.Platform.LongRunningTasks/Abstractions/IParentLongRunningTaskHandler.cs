namespace JK.Platform.LongRunningTasks.Abstractions;

/// <summary>
/// Marker for orchestrator handlers that spawn child chunk tasks.
/// The worker does not mark these tasks as Completed after ExecuteAsync.
/// </summary>
public interface IParentLongRunningTaskHandler : ILongRunningTaskHandler;
