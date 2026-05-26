using AutoMapper;
using AutoMapper.QueryableExtensions;
using JK.Offer.Contracts;
using JK.Offer.Database.Entities;
using JK.Offer.Models;
using JK.Offer.Tasks.External;
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

    public async Task<(long Processed, long Failed)> UpsertExternalOffersBatchAsync(
        IReadOnlyList<ExternalOfferDto> offers,
        CancellationToken cancellationToken = default)
    {
        if (offers.Count == 0)
            return (0, 0);

        var numbers = offers.Select(o => o.Number).ToList();
        var existing = await DbSet
            .Where(o => numbers.Contains(o.Number))
            .ToDictionaryAsync(o => o.Number, cancellationToken);

        long processed = 0;
        long failed = 0;
        var now = DateTime.UtcNow;

        foreach (var external in offers)
        {
            try
            {
                if (existing.TryGetValue(external.Number, out var entity))
                {
                    entity.TotalAmount = external.TotalAmount;
                    entity.Status = external.Status;
                    entity.ExpiresAt = external.ExpiresAt;
                    entity.UpdatedAt = now;
                }
                else
                {
                    await DbSet.AddAsync(new OfferEntity
                    {
                        Id = Guid.NewGuid(),
                        Number = external.Number,
                        TotalAmount = external.TotalAmount,
                        Status = external.Status,
                        ExpiresAt = external.ExpiresAt,
                        CreatedAt = now
                    }, cancellationToken);
                }

                processed++;
            }
            catch
            {
                failed++;
            }
        }

        await Context.SaveChangesAsync(cancellationToken);
        return (processed, failed);
    }
}
