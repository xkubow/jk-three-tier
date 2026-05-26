using JK.Platform.LongRunningTasks.Abstractions;
using JK.Platform.LongRunningTasks.Entities;

namespace JK.Offer.Tasks;

public class TestLongRunningTaskHandler : ILongRunningTaskHandler
{
    public const string TaskNameValue = "Test";

    public string TaskName => TaskNameValue;

    public async Task ExecuteAsync(LongRunningTaskEntity task, CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken);
    }
}
