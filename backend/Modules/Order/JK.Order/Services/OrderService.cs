using JK.Order.Contracts;
using JK.Order.Database;
using JK.Order.Database.Entities;
using JK.Platform.Core.DependencyInjection.Attributes;

namespace JK.Order.Services;

[Injectable]
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);

    public Task<PagedResponse<OrderDto>> ListAsync(ListOrdersRequest request, CancellationToken cancellationToken = default)
        => _unitOfWork.Orders.ListAsync(request, cancellationToken);

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new OrderEntity
        {
            Id = Guid.NewGuid(),
            Number = request.Number.Trim(),
            TotalAmount = request.TotalAmount,
            Status = "New",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _unitOfWork.Orders.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _unitOfWork.Orders.GetByIdAsync(entity.Id, cancellationToken);
        return dto!;
    }

    public async Task<OrderDto?> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Orders.GetEntityByIdAsync(id, cancellationToken);
        if (entity == null) return null;

        entity.Status = request.Status.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Orders.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Orders.GetEntityByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _unitOfWork.Orders.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}


