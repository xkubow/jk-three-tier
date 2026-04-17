using JK.Messaging.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JK.Messaging.Database;

public class MessagingDbContext : DbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options)
        : base(options)
    {
    }

    public DbSet<MessagingEntity> Messaging { get; set; } = null!;
    public DbSet<ApiMessageTaskEntity> ApiMessageTasks { get; set; } = null!;
}
