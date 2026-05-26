using JK.Platform.LongRunningTasks.Entities;
using JK.Platform.LongRunningTasks.Enums;
using Microsoft.EntityFrameworkCore;

namespace JK.Platform.LongRunningTasks.Extensions;

public static class LongRunningTasksModelBuilderExtensions
{
    public static void ConfigureLongRunningTasks(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LongRunningTaskEntity>(entity =>
        {
            entity.Property(e => e.Status)
                .HasConversion<string>();

            entity.HasIndex(e => new { e.Status, e.NextRunAtUtc });
            entity.HasIndex(e => e.ParentTaskId);
            entity.HasIndex(e => e.TaskName);
            entity.HasIndex(e => e.CorrelationId);
        });
    }
}
