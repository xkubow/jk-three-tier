using JK.Order.Database.Repositories;

namespace JK.Order.Database;

public interface IUnitOfWork
{
    IOrderRepository Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

