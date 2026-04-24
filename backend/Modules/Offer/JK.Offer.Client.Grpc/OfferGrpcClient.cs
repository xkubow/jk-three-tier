using Grpc.Net.Client;
using JK.Offer.Contracts;
using JK.Offer.Proto;
using CreateOfferRequest = JK.Offer.Contracts.CreateOfferRequest;
using ListOffersRequest = JK.Offer.Contracts.ListOffersRequest;
using UpdateOfferRequest = JK.Offer.Contracts.UpdateOfferRequest;

namespace JK.Offer.Client.Grpc;

public class OfferGrpcClient : IOfferGrpcClient
{
    private readonly OfferGrpc.OfferGrpcClient _client;

    public OfferGrpcClient(OfferGrpc.OfferGrpcClient client)
    {
        _client = client;
    }

    public async Task<OfferDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetByIdAsync(
            new GetOfferByIdRequest { Id = id.ToString() },
            cancellationToken: cancellationToken);
        return FromMessage(response);
    }

    public async Task<PagedResponse<OfferDto>> ListAsync(ListOffersRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.ListOffersRequest
        {
            SearchTerm = request.SearchTerm ?? string.Empty,
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy ?? string.Empty,
            SortDirection = request.SortDirection
        };

        var response = await _client.ListAsync(protoRequest, cancellationToken: cancellationToken);
        var items = response.Items.Select(FromMessage).Where(x => x != null).Cast<OfferDto>().ToList();
        return new PagedResponse<OfferDto>
        {
            Items = items,
            Page = response.Page,
            PageSize = response.PageSize,
            TotalCount = response.TotalCount
        };
    }

    public async Task<OfferDto> CreateAsync(CreateOfferRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.CreateOfferRequest
        {
            Number = request.Number,
            TotalAmount = (double)request.TotalAmount,
            ExpiresAt = request.ExpiresAt.ToString("O")
        };

        var response = await _client.CreateAsync(protoRequest, cancellationToken: cancellationToken);
        return FromMessage(response)!;
    }

    public async Task<OfferDto?> UpdateAsync(Guid id, UpdateOfferRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.UpdateOfferRequest
        {
            Id = id.ToString(),
            Status = request.Status,
            TotalAmount = (double)request.TotalAmount,
            ExpiresAt = request.ExpiresAt.ToString("O")
        };

        var response = await _client.UpdateAsync(protoRequest, cancellationToken: cancellationToken);
        return FromMessage(response);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _client.DeleteAsync(
            new DeleteOfferRequest { Id = id.ToString() },
            cancellationToken: cancellationToken);
        return response.Success;
    }

    private static OfferDto? FromMessage(OfferMessage msg)
    {
        if (string.IsNullOrEmpty(msg.Id) || !Guid.TryParse(msg.Id, out var id))
            return null;
        if (!DateTime.TryParse(msg.CreatedAt, out var createdAt)) createdAt = DateTime.UtcNow;
        if (!DateTime.TryParse(msg.UpdatedAt, out var updatedAt)) updatedAt = createdAt;
        if (!DateTime.TryParse(msg.ExpiresAt, out var expiresAt)) expiresAt = createdAt.AddDays(30);

        return new OfferDto
        {
            Id = id,
            Number = msg.Number,
            TotalAmount = (decimal)msg.TotalAmount,
            Status = msg.Status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            ExpiresAt = expiresAt
        };
    }
}
