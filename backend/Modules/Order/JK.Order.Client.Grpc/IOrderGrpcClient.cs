using JK.Order.Contracts;

namespace JK.Order.Client.Grpc;

public interface IOrderGrpcClient
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderDto>> ListAsync(Contracts.ListOrdersRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(Contracts.CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto?> UpdateAsync(Guid id, Contracts.UpdateOrderRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

