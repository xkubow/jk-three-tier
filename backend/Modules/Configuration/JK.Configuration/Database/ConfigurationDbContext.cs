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
}
