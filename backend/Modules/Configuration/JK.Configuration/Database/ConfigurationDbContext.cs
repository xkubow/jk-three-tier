using JK.Configuration.Database.Entities;
using JK.Platform.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace JK.Configuration.Database;

public class ConfigurationDbContext : DbContextBase
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigurationEntity> Configurations { get; set; }
}
