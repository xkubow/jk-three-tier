using JK.Order.Database.Repositories;
using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Order.Database;

[Injectable(ServiceLifetime.Scoped)]
public class UnitOfWork : UnitOfWork<OrderDbContext>, IUnitOfWork
{
    public UnitOfWork(OrderDbContext context, IOrderRepository orders) : base(context)
    {
        Orders = orders;
    }

    public IOrderRepository Orders { get; }
}

