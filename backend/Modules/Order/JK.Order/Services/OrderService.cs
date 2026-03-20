using AutoMapper;
using JK.Order.Configurations;
using JK.Order.Contracts;
using JK.Order.Database;
using JK.Order.Database.Entities;
using JK.Order.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JK.Order.Services;

[Injectable(ServiceLifetime.Scoped)]
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IOptionsSnapshot<OrderConfiguration> _configuration;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IOptionsSnapshot<OrderConfiguration> configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(_configuration.Value.RetryCount);
        var model = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<OrderDto>(model);
    }

    public async Task<PagedResponse<OrderDto>> ListAsync(ListOrdersRequest request, CancellationToken cancellationToken = default)
    {
        var pagedResponse = await _unitOfWork.Orders.ListAsync(request, cancellationToken);
        return _mapper.Map<PagedResponse<OrderDto>>(pagedResponse);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var model = new OrderModel
        {
            Id = Guid.NewGuid(),
            Number = request.Number.Trim(),
            TotalAmount = request.TotalAmount,
            Status = "New",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Orders.AddAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var insertedModel = await _unitOfWork.Orders.GetByIdAsync(model.Id, cancellationToken);
        return _mapper.Map<OrderDto>(insertedModel);
    }

    public async Task<OrderDto?> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var model = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (model == null) return null;

        model.Status = request.Status.Trim();
        model.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedModel = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<OrderDto>(updatedModel);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Orders.ExistsAsync(id, cancellationToken);
        if (!exists) return false;

        await _unitOfWork.Orders.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}


