using Backend.Database.Entities;
using JK.Platform.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public class ConfigDbContext : DbContextBase
{
    public ConfigDbContext(DbContextOptions<ConfigDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigurationEntity> Configuration { get; set; }
}