using JK.Playground.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public class ConfigDbContext : DbContext
{
    public ConfigDbContext(DbContextOptions<ConfigDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigurationEntity> Configuration { get; set; }
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderProductEntity> OrderProducts { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
}