using JK.Offer.Database.Entities;
using JK.Platform.LongRunningTasks.Abstractions;
using JK.Platform.LongRunningTasks.Entities;
using JK.Platform.LongRunningTasks.Extensions;
using JK.Platform.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace JK.Offer.Database;

public class OfferDbContext : DbContextBase, ILongRunningTasksDbContext
{
    public OfferDbContext(DbContextOptions<OfferDbContext> options)
        : base(options)
    {
    }

    public DbSet<OfferEntity> Offers { get; set; } = null!;

    public DbSet<LongRunningTaskEntity> LongRunningTasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureLongRunningTasks();
        base.OnModelCreating(modelBuilder);
    }
}
