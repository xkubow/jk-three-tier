using JK.Offer.Contracts;

namespace JK.Offer.Client.Grpc;

public interface IOfferGrpcClient
{
    Task<OfferDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<OfferDto>> ListAsync(ListOffersRequest request, CancellationToken cancellationToken = default);
    Task<OfferDto> CreateAsync(CreateOfferRequest request, CancellationToken cancellationToken = default);
    Task<OfferDto?> UpdateAsync(Guid id, UpdateOfferRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
