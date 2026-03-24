using JK.Configuration.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JK.Configuration.Database;

public class ConfigurationDbContext : DbContext
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigurationEntity> Configurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConfigurationEntity>(e =>
        {
            e.HasIndex(x => new { x.MarketCode, x.ServiceCode, x.Key })
                .HasFilter("\"IsDeleted\" = false");
        });
    }
}
