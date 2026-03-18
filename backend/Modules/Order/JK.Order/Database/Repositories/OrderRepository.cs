using AutoMapper;
using AutoMapper.QueryableExtensions;
using JK.Order.Contracts;
using JK.Order.Database.Entities;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.EntityFrameworkCore;

namespace JK.Order.Database.Repositories;

[Injectable]
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;

    public OrderRepository(OrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<OrderDto>> ListAsync(ListOrdersRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsNoTracking();

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
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResponse<OrderDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public void Add(OrderEntity entity)
    {
        _context.Orders.Add(entity);
    }

    public void Update(OrderEntity entity)
    {
        _context.Orders.Update(entity);
    }

    public void Delete(OrderEntity entity)
    {
        _context.Orders.Remove(entity);
    }
}

