using JK.Platform.LongRunningTasks.Abstractions;

namespace JK.Platform.LongRunningTasks.Services;

public class LongRunningTaskHandlerRegistry
{
    private readonly IReadOnlyDictionary<string, ILongRunningTaskHandler> _handlersByName;

    public LongRunningTaskHandlerRegistry(IEnumerable<ILongRunningTaskHandler> handlers)
    {
        _handlersByName = handlers.ToDictionary(h => h.TaskName, StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetHandler(string taskName, out ILongRunningTaskHandler? handler)
    {
        return _handlersByName.TryGetValue(taskName, out handler);
    }
}
