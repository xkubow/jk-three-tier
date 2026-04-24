using JK.Offer.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JK.Offer.Database;

public class OfferDbContext : DbContext
{
    public OfferDbContext(DbContextOptions<OfferDbContext> options)
        : base(options)
    {
    }

    public DbSet<OfferEntity> Offers { get; set; } = null!;
}
