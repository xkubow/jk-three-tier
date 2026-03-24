using Backend.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public class ConfigDbContext : DbContext
{
    public ConfigDbContext(DbContextOptions<ConfigDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigurationEntity> Configuration { get; set; }
}