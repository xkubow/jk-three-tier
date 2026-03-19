using JK.Order.Contracts;
using JK.Order.Database.Entities;
using JK.Order.Models;
using JK.Platform.Persistence.EfCore;

namespace JK.Order.Database.Repositories;

public interface IOrderRepository : IRepository<OrderModel, Guid>
{
    Task<OrderEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderModel>> ListAsync(ListOrdersRequest request, CancellationToken cancellationToken = default);
}

