using Grpc.Net.Client;
using JK.Order.Contracts;
using JK.Order.Proto;
using CreateOrderRequest = JK.Order.Contracts.CreateOrderRequest;
using ListOrdersRequest = JK.Order.Contracts.ListOrdersRequest;
using UpdateOrderRequest = JK.Order.Contracts.UpdateOrderRequest;

namespace JK.Order.Client.Grpc;

public class OrderGrpcClient : IOrderGrpcClient
{
    private readonly OrderGrpc.OrderGrpcClient _client;

    public OrderGrpcClient(GrpcChannel channel)
    {
        _client = new OrderGrpc.OrderGrpcClient(channel);
    }

    public OrderGrpcClient(string address) : this(GrpcChannel.ForAddress(address))
    {
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetByIdAsync(
            new GetOrderByIdRequest { Id = id.ToString() },
            cancellationToken: cancellationToken);
        return FromMessage(response);
    }

    public async Task<PagedResponse<OrderDto>> ListAsync(ListOrdersRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.ListOrdersRequest
        {
            SearchTerm = request.SearchTerm ?? string.Empty,
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy ?? string.Empty,
            SortDirection = request.SortDirection
        };

        var response = await _client.ListAsync(protoRequest, cancellationToken: cancellationToken);
        var items = response.Items.Select(FromMessage).Where(x => x != null).Cast<OrderDto>().ToList();
        return new PagedResponse<OrderDto>
        {
            Items = items,
            Page = response.Page,
            PageSize = response.PageSize,
            TotalCount = response.TotalCount
        };
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.CreateOrderRequest
        {
            Number = request.Number,
            TotalAmount = (double)request.TotalAmount
        };

        var response = await _client.CreateAsync(protoRequest, cancellationToken: cancellationToken);
        return FromMessage(response)!;
    }

    public async Task<OrderDto?> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.UpdateOrderRequest
        {
            Id = id.ToString(),
            Status = request.Status
        };

        var response = await _client.UpdateAsync(protoRequest, cancellationToken: cancellationToken);
        return FromMessage(response);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _client.DeleteAsync(
            new DeleteOrderRequest { Id = id.ToString() },
            cancellationToken: cancellationToken);
        return response.Success;
    }

    private static OrderDto? FromMessage(OrderMessage msg)
    {
        if (string.IsNullOrEmpty(msg.Id) || !Guid.TryParse(msg.Id, out var id))
            return null;
        if (!DateTime.TryParse(msg.CreatedAt, out var createdAt)) createdAt = DateTime.UtcNow;
        if (!DateTime.TryParse(msg.UpdatedAt, out var updatedAt)) updatedAt = createdAt;

        return new OrderDto
        {
            Id = id,
            Number = msg.Number,
            TotalAmount = (decimal)msg.TotalAmount,
            Status = msg.Status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}

