using JK.Order.Database.Entities;
using JK.Platform.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace JK.Order.Database;

public class OrderDbContext : DbContextBase
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderEntity> Orders { get; set; } = null!;
}

