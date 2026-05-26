using JK.Platform.LongRunningTasks.Entities;
using Microsoft.EntityFrameworkCore;

namespace JK.Platform.LongRunningTasks.Abstractions;

public interface ILongRunningTasksDbContext
{
    DbSet<LongRunningTaskEntity> LongRunningTasks { get; }
}
