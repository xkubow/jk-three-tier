using Grpc.Core;
using JK.Messaging.Proto;
using JK.Messaging.Services;
using Contracts = JK.Messaging.Contracts;
using ProtoCreate = JK.Messaging.Proto.CreateMessagingRequest;
using ProtoList = JK.Messaging.Proto.ListMessagingRequest;
using ProtoUpdate = JK.Messaging.Proto.UpdateMessagingRequest;

namespace JK.Messaging.Grpc;

public class MessagingGrpcService : MessagingGrpc.MessagingGrpcBase
{
    private readonly IMessagingService _service;

    public MessagingGrpcService(IMessagingService service)
    {
        _service = service;
    }

    public override async Task<MessagingMessage> GetById(GetMessagingByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));

        var item = await _service.GetByIdAsync(id, context.CancellationToken);
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Record not found"));

        return ToMessage(item);
    }

    public override async Task<ListMessagingResponse> List(ProtoList request, ServerCallContext context)
    {
        var listRequest = new Contracts.ListMessagingRequest
        {
            SearchTerm = request.SearchTerm,
            Page = request.Page > 0 ? request.Page : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 20,
            SortBy = request.SortBy,
            SortDirection = string.IsNullOrEmpty(request.SortDirection) ? "asc" : request.SortDirection
        };

        var result = await _service.ListAsync(listRequest, context.CancellationToken);

        var response = new ListMessagingResponse
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
        response.Items.AddRange(result.Items.Select(ToMessage));
        return response;
    }

    public override async Task<MessagingMessage> Create(ProtoCreate request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ThreadId, out var threadId) || !Guid.TryParse(request.SenderId, out var senderId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ThreadId or SenderId"));

        var createRequest = new Contracts.CreateMessagingRequest
        {
            ThreadId = threadId,
            SenderId = senderId,
            Content = request.Content
        };

        var item = await _service.CreateAsync(createRequest, context.CancellationToken);
        return ToMessage(item);
    }

    public override async Task<MessagingMessage> Update(ProtoUpdate request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));

        var updateRequest = new Contracts.UpdateMessagingRequest
        {
            Status = request.Status
        };

        var item = await _service.UpdateAsync(id, updateRequest, context.CancellationToken);
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Record not found"));

        return ToMessage(item);
    }

    public override async Task<DeleteMessagingResponse> Delete(DeleteMessagingRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));

        var deleted = await _service.DeleteAsync(id, context.CancellationToken);
        return new DeleteMessagingResponse { Success = deleted };
    }

    public override async Task<EchoViaOrleansResponse> EchoViaOrleans(
        EchoViaOrleansRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.ThreadId, out var threadId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ThreadId"));

        var echoRequest = new Contracts.EchoViaOrleansRequest
        {
            ThreadId = threadId,
            SenderId = request.SenderId,
            Content = request.Content
        };

        var result = await _service.EchoViaOrleansAsync(echoRequest, context.CancellationToken);
        return new EchoViaOrleansResponse { Result = result };
    }

    public override async Task<RegisterApiMessageResponse> RegisterApiMessage(
        RegisterApiMessageRequest request,
        ServerCallContext context)
    {
        var registerRequest = new Contracts.RegisterApiMessageRequest
        {
            Cron = request.Cron
        };

        var result = await _service.RegisterApiMessageAsync(registerRequest, context.CancellationToken);
        return new RegisterApiMessageResponse { Success = result };
    }

    public override async Task<SendApiMessageResponse> SendApiMessage(
        SendApiMessageRequest request,
        ServerCallContext context)
    {
        var sendRequest = new Contracts.SendApiMessageRequest
        {
            Url = request.Url,
            GrainId = request.HasGrainId ? request.GrainId : null
        };

        await _service.SendApiMessageAsync(sendRequest, context.CancellationToken);
        return new SendApiMessageResponse();
    }

    private static MessagingMessage ToMessage(Contracts.MessagingDto dto)
    {
        return new MessagingMessage
        {
            Id = dto.Id.ToString(),
            ThreadId = dto.ThreadId.ToString(),
            SenderId = dto.SenderId.ToString(),
            Content = dto.Content,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt.ToString("O"),
            UpdatedAt = dto.UpdatedAt.ToString("O")
        };
    }
}
