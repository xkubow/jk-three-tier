using JK.Offer.Contracts;
using JK.Offer.Database.Entities;
using JK.Offer.Models;
using JK.Platform.Persistence.EfCore;

namespace JK.Offer.Database.Repositories;

public interface IOfferRepository : IRepository<OfferModel, Guid>
{
    Task<OfferEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<OfferModel>> ListAsync(ListOffersRequest request, CancellationToken cancellationToken = default);
}
