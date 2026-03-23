using Grpc.Core;
using JK.Order.Contracts;
using JK.Order.Proto;
using JK.Order.Services;
using CreateOrderRequest = JK.Order.Proto.CreateOrderRequest;
using ListOrdersRequest = JK.Order.Proto.ListOrdersRequest;
using UpdateOrderRequest = JK.Order.Proto.UpdateOrderRequest;

namespace JK.Order.Grpc;

public class OrderGrpcService : OrderGrpc.OrderGrpcBase
{
    private readonly IOrderService _service;

    public OrderGrpcService(IOrderService service)
    {
        _service = service;
    }

    public override async Task<OrderMessage> GetById(GetOrderByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));

        var item = await _service.GetByIdAsync(id, context.CancellationToken);
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

        return ToMessage(item);
    }

    public override async Task<ListOrdersResponse> List(ListOrdersRequest request, ServerCallContext context)
    {
        var listRequest = new Contracts.ListOrdersRequest
        {
            SearchTerm = request.SearchTerm,
            Page = request.Page > 0 ? request.Page : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 20,
            SortBy = request.SortBy,
            SortDirection = string.IsNullOrEmpty(request.SortDirection) ? "asc" : request.SortDirection
        };

        var result = await _service.ListAsync(listRequest, context.CancellationToken);

        var response = new ListOrdersResponse
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
        response.Items.AddRange(result.Items.Select(ToMessage));
        return response;
    }

    public override async Task<OrderMessage> Create(CreateOrderRequest request, ServerCallContext context)
    {
        var createRequest = new Contracts.CreateOrderRequest
        {
            Number = request.Number,
            TotalAmount = (decimal)request.TotalAmount
        };

        var item = await _service.CreateAsync(createRequest, context.CancellationToken);
        return ToMessage(item);
    }

    public override async Task<OrderMessage> Update(UpdateOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));

        var updateRequest = new Contracts.UpdateOrderRequest
        {
            Status = request.Status
        };

        var item = await _service.UpdateAsync(id, updateRequest, context.CancellationToken);
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

        return ToMessage(item);
    }

    public override async Task<DeleteOrderResponse> Delete(DeleteOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));

        var deleted = await _service.DeleteAsync(id, context.CancellationToken);
        return new DeleteOrderResponse { Success = deleted };
    }

    private static OrderMessage ToMessage(OrderDto dto)
    {
        return new OrderMessage
        {
            Id = dto.Id.ToString(),
            Number = dto.Number,
            TotalAmount = (double)dto.TotalAmount,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt.ToString("O"),
            UpdatedAt = dto.UpdatedAt.ToString("O")
        };
    }
}

