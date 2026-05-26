using JK.Messaging.Database.Entities;
using JK.Platform.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace JK.Messaging.Database;

public class MessagingDbContext : DbContextBase
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApiMessageTaskEntity> ApiMessageTasks { get; set; } = null!;
    public DbSet<ApiMessageRecurringTaskEntity> ApiMessageRecurringTasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiMessageTaskEntity>(entity =>
        {
            entity.Property(e => e.State)
                .HasConversion<string>();
        });

        base.OnModelCreating(modelBuilder);
    }
}
