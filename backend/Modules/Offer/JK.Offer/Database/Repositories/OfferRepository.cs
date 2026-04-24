using AutoMapper;
using AutoMapper.QueryableExtensions;
using JK.Offer.Contracts;
using JK.Offer.Database.Entities;
using JK.Offer.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JK.Platform.Persistence.EfCore;

namespace JK.Offer.Database.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class OfferRepository : BaseRepository<OfferModel, OfferEntity, Guid>, IOfferRepository
{
    public OfferRepository(OfferDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<OfferEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.Id.Equals(id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<OfferModel>> ListAsync(ListOffersRequest request, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(o =>
                o.Number.ToLower().Contains(term) ||
                o.Status.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sortBy = request.SortBy?.ToLowerInvariant() ?? "createdat";
        var ascending = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = sortBy switch
        {
            "number" => ascending ? query.OrderBy(o => o.Number) : query.OrderByDescending(o => o.Number),
            "totalamount" => ascending ? query.OrderBy(o => o.TotalAmount) : query.OrderByDescending(o => o.TotalAmount),
            "status" => ascending ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
            "updatedat" => ascending ? query.OrderBy(o => o.UpdatedAt) : query.OrderByDescending(o => o.UpdatedAt),
            "expiresat" => ascending ? query.OrderBy(o => o.ExpiresAt) : query.OrderByDescending(o => o.ExpiresAt),
            _ => ascending ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt)
        };

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<OfferModel>(Mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResponse<OfferModel>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
