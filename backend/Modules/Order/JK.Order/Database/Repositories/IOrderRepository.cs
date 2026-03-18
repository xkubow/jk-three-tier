using JK.Order.Contracts;
using JK.Order.Database.Entities;

namespace JK.Order.Database.Repositories;

public interface IOrderRepository
{
    Task<OrderEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderDto>> ListAsync(ListOrdersRequest request, CancellationToken cancellationToken = default);
    void Add(OrderEntity entity);
    void Update(OrderEntity entity);
    void Delete(OrderEntity entity);
}

