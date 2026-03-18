using JK.Order.Database.Repositories;
using JK.Platform.Core.DependencyInjection.Attributes;

namespace JK.Order.Database;

[Injectable]
public class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;

    public UnitOfWork(OrderDbContext context, IOrderRepository orders)
    {
        _context = context;
        Orders = orders;
    }

    public IOrderRepository Orders { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

