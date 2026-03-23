using JK.Order.Database.Repositories;

namespace JK.Order.Database;

public interface IUnitOfWork : Platform.Core.Persistence.IUnitOfWork
{
    IOrderRepository Orders { get; }
}

