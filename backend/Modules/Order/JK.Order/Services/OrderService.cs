using AutoMapper;
using JK.Messaging.Client.Grpc;
using JK.Messaging.Contracts;
using JK.Order.Configurations;
using JK.Order.Contracts;
using JK.Order.Database.Repositories;
using JK.Order.Database;
using JK.Platform.Persistence.EfCore;
using JK.Order.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JK.Order.Services;

[Injectable(ServiceLifetime.Scoped)]
public class OrderService : IOrderService
{
    private readonly IUnitOfWork<OrderDbContext> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IOptionsSnapshot<OrderConfiguration> _configuration;
    private readonly IApiMessageTaskGrpcClient _apiMessageTaskGrpcClient;

    public OrderService(IUnitOfWorkFactory<OrderDbContext> unitOfWorkFactory, IMapper mapper, IOptionsSnapshot<OrderConfiguration> configuration, IApiMessageTaskGrpcClient apiMessageTaskGrpcClient)
    {
        _unitOfWork = unitOfWorkFactory.Create();
        _mapper = mapper;
        _configuration = configuration;
        _apiMessageTaskGrpcClient = apiMessageTaskGrpcClient;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(_configuration.Value.RetryCount);
        var model = await _unitOfWork.GetRepository<IOrderRepository>().GetByIdAsync(id, cancellationToken);
        return _mapper.Map<OrderDto>(model);
    }

    public async Task<Contracts.PagedResponse<OrderDto>> ListAsync(ListOrdersRequest request, CancellationToken cancellationToken = default)
    {
        var pagedResponse = await _unitOfWork.GetRepository<IOrderRepository>().ListAsync(request, cancellationToken);
        return _mapper.Map<Contracts.PagedResponse<OrderDto>>(pagedResponse);
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

        await _unitOfWork.GetRepository<IOrderRepository>().AddAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var insertedModel = await _unitOfWork.GetRepository<IOrderRepository>().GetByIdAsync(model.Id, cancellationToken);
        return _mapper.Map<OrderDto>(insertedModel);
    }

    public async Task<OrderDto?> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var model = await _unitOfWork.GetRepository<IOrderRepository>().GetByIdAsync(id, cancellationToken);
        if (model == null) return null;

        model.Status = request.Status.Trim();
        model.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.GetRepository<IOrderRepository>().UpdateAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedModel = await _unitOfWork.GetRepository<IOrderRepository>().GetByIdAsync(id, cancellationToken);
        return _mapper.Map<OrderDto>(updatedModel);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.GetRepository<IOrderRepository>().ExistsAsync(id, cancellationToken);
        if (!exists) return false;

        await _unitOfWork.GetRepository<IOrderRepository>().DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task Test()
    {
        await _apiMessageTaskGrpcClient.CreateAsync(new CreateApiMessageTaskRequest() { TaskId = Guid.NewGuid().ToString() + "_order_test", TaskName = "Test", TargetUrl = "grpcs://localhost:7005/jk.offer.OfferGrpc/Test", MaxAttempts = 3 });
        Console.WriteLine("Test from Order Service");
    }
}


