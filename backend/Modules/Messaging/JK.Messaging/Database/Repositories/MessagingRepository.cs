using AutoMapper;
using AutoMapper.QueryableExtensions;
using JK.Messaging.Contracts;
using JK.Messaging.Database.Entities;
using JK.Messaging.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Messaging.Database.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class MessagingRepository : BaseRepository<MessagingModel, MessagingEntity, Guid>, IMessagingRepository
{
    public MessagingRepository(MessagingDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<MessagingEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.Id.Equals(id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<MessagingModel>> ListAsync(ListMessagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(m =>
                m.Content.ToLower().Contains(term) ||
                m.Status.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sortBy = request.SortBy?.ToLowerInvariant() ?? "createdat";
        var ascending = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = sortBy switch
        {
            "content" => ascending ? query.OrderBy(m => m.Content) : query.OrderByDescending(m => m.Content),
            "status" => ascending ? query.OrderBy(m => m.Status) : query.OrderByDescending(m => m.Status),
            "threadid" => ascending ? query.OrderBy(m => m.ThreadId) : query.OrderByDescending(m => m.ThreadId),
            "senderid" => ascending ? query.OrderBy(m => m.SenderId) : query.OrderByDescending(m => m.SenderId),
            "updatedat" => ascending ? query.OrderBy(m => m.UpdatedAt) : query.OrderByDescending(m => m.UpdatedAt),
            _ => ascending ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt)
        };

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<MessagingModel>(Mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResponse<MessagingModel>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
