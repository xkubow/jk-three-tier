using Grpc.Net.Client;
using JK.Messaging.Contracts;
using JK.Messaging.Proto;
using CreateMessagingRequest = JK.Messaging.Contracts.CreateMessagingRequest;
using EchoViaOrleansRequest = JK.Messaging.Contracts.EchoViaOrleansRequest;
using ListMessagingRequest = JK.Messaging.Contracts.ListMessagingRequest;
using UpdateMessagingRequest = JK.Messaging.Contracts.UpdateMessagingRequest;

namespace JK.Messaging.Client.Grpc;

public class MessagingGrpcClient : IMessagingGrpcClient
{
    private readonly MessagingGrpc.MessagingGrpcClient _client;

    public MessagingGrpcClient(GrpcChannel channel)
    {
        _client = new MessagingGrpc.MessagingGrpcClient(channel);
    }

    public MessagingGrpcClient(string address) : this(GrpcChannel.ForAddress(address))
    {
    }

    public async Task<MessagingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetByIdAsync(
            new GetMessagingByIdRequest { Id = id.ToString() },
            cancellationToken: cancellationToken);
        return FromMessage(response);
    }

    public async Task<PagedResponse<MessagingDto>> ListAsync(ListMessagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.ListMessagingRequest
        {
            SearchTerm = request.SearchTerm ?? string.Empty,
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy ?? string.Empty,
            SortDirection = request.SortDirection
        };

        var response = await _client.ListAsync(protoRequest, cancellationToken: cancellationToken);
        var items = response.Items.Select(FromMessage).Where(x => x != null).Cast<MessagingDto>().ToList();
        return new PagedResponse<MessagingDto>
        {
            Items = items,
            Page = response.Page,
            PageSize = response.PageSize,
            TotalCount = response.TotalCount
        };
    }

    public async Task<MessagingDto> CreateAsync(CreateMessagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.CreateMessagingRequest
        {
            ThreadId = request.ThreadId.ToString(),
            SenderId = request.SenderId.ToString(),
            Content = request.Content
        };

        var response = await _client.CreateAsync(protoRequest, cancellationToken: cancellationToken);
        return FromMessage(response)!;
    }

    public async Task<MessagingDto?> UpdateAsync(Guid id, UpdateMessagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.UpdateMessagingRequest
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
            new DeleteMessagingRequest { Id = id.ToString() },
            cancellationToken: cancellationToken);
        return response.Success;
    }

    public async Task<string> EchoViaOrleansAsync(EchoViaOrleansRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.EchoViaOrleansAsync(
            new Proto.EchoViaOrleansRequest
            {
                ThreadId = request.ThreadId.ToString(),
                SenderId = request.SenderId,
                Content = request.Content
            },
            cancellationToken: cancellationToken);
        return response.Result;
    }

    private static MessagingDto? FromMessage(MessagingMessage msg)
    {
        if (string.IsNullOrEmpty(msg.Id) || !Guid.TryParse(msg.Id, out var id))
            return null;
        if (!Guid.TryParse(msg.ThreadId, out var threadId)) threadId = Guid.Empty;
        if (!Guid.TryParse(msg.SenderId, out var senderId)) senderId = Guid.Empty;
        if (!DateTime.TryParse(msg.CreatedAt, out var createdAt)) createdAt = DateTime.UtcNow;
        if (!DateTime.TryParse(msg.UpdatedAt, out var updatedAt)) updatedAt = createdAt;

        return new MessagingDto
        {
            Id = id,
            ThreadId = threadId,
            SenderId = senderId,
            Content = msg.Content,
            Status = msg.Status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
