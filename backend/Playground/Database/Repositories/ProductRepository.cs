using Backend.Database;
using JK.Playground.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JK.Playground.Database.Repositories;

public class ProductRepository: IProductRepository
{
    private readonly ConfigDbContext _context;
    private readonly DbSet<ProductEntity> _products;

    public ProductRepository(ConfigDbContext context)
    {
        _context = context;
        _products = context.Products;
    }

    public async Task<bool> ExistsAmountAsync(IDictionary<Guid, int> productsToCheck)
        => await _products.Where(p => productsToCheck.Keys.Contains(p.Id)).AnyAsync(q => productsToCheck[q.Id] <= q.Quantity);

}
