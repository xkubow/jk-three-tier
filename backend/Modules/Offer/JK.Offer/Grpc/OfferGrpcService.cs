using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using JK.Offer.Contracts;
using JK.Offer.Proto;
using JK.Offer.Services;
using CreateOfferRequest = JK.Offer.Proto.CreateOfferRequest;
using ListOffersRequest = JK.Offer.Proto.ListOffersRequest;
using UpdateOfferRequest = JK.Offer.Proto.UpdateOfferRequest;

namespace JK.Offer.Grpc;

public class OfferGrpcService : OfferGrpc.OfferGrpcBase
{
    private readonly IOfferService _service;

    public OfferGrpcService(IOfferService service)
    {
        _service = service;
    }

    public override async Task<OfferMessage> GetById(GetOfferByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));

        var item = await _service.GetByIdAsync(id, context.CancellationToken);
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Offer not found"));

        return ToMessage(item);
    }

    public override async Task<ListOffersResponse> List(ListOffersRequest request, ServerCallContext context)
    {
        var listRequest = new Contracts.ListOffersRequest
        {
            SearchTerm = request.SearchTerm,
            Page = request.Page > 0 ? request.Page : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 20,
            SortBy = request.SortBy,
            SortDirection = string.IsNullOrEmpty(request.SortDirection) ? "asc" : request.SortDirection
        };

        var result = await _service.ListAsync(listRequest, context.CancellationToken);

        var response = new ListOffersResponse
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
        response.Items.AddRange(result.Items.Select(ToMessage));
        return response;
    }

    public override async Task<OfferMessage> Create(CreateOfferRequest request, ServerCallContext context)
    {
        var createRequest = new Contracts.CreateOfferRequest
        {
            Number = request.Number,
            TotalAmount = (decimal)request.TotalAmount,
            ExpiresAt = DateTime.Parse(request.ExpiresAt)
        };

        var item = await _service.CreateAsync(createRequest, context.CancellationToken);
        return ToMessage(item);
    }

    public override async Task<OfferMessage> Update(UpdateOfferRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));

        var updateRequest = new Contracts.UpdateOfferRequest
        {
            Status = request.Status,
            TotalAmount = (decimal)request.TotalAmount,
            ExpiresAt = DateTime.Parse(request.ExpiresAt)
        };

        var item = await _service.UpdateAsync(id, updateRequest, context.CancellationToken);
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Offer not found"));

        return ToMessage(item);
    }

    public override async Task<DeleteOfferResponse> Delete(DeleteOfferRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));

        var deleted = await _service.DeleteAsync(id, context.CancellationToken);
        return new DeleteOfferResponse { Success = deleted };
    }

    private static OfferMessage ToMessage(OfferDto dto)
    {
        return new OfferMessage
        {
            Id = dto.Id.ToString(),
            Number = dto.Number,
            TotalAmount = (double)dto.TotalAmount,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt.ToString("O"),
            UpdatedAt = dto.UpdatedAt.ToString("O"),
            ExpiresAt = dto.ExpiresAt.ToString("O")
        };
    }

    public override Task<Empty> Test(Empty request, ServerCallContext context)
    {
        _service.Test();
        return Task.FromResult(new Empty());
    }
}
