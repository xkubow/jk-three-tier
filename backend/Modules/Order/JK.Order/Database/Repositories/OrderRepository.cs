using AutoMapper;
using AutoMapper.QueryableExtensions;
using JK.Order.Contracts;
using JK.Order.Database.Entities;
using JK.Order.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Order.Database.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class OrderRepository : BaseRepository<OrderModel, OrderEntity, Guid>, IOrderRepository
{
    public OrderRepository(OrderDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<OrderEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.Id.Equals(id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<OrderModel>> ListAsync(ListOrdersRequest request, CancellationToken cancellationToken = default)
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
            _ => ascending ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt)
        };

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<OrderModel>(Mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResponse<OrderModel>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

