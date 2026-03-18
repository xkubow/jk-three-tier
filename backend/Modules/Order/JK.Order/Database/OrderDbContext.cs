using JK.Order.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JK.Order.Database;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderEntity> Orders { get; set; } = null!;
}

