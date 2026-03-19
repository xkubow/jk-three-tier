using AutoMapper;
using AutoMapper.QueryableExtensions;
using JK.Configuration.Contracts;
using JK.Configuration.Database.Entities;
using JK.Configuration.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.EntityFrameworkCore;

namespace JK.Configuration.Database.Repositories;

[Injectable]
public class ConfigurationRepository : IConfigurationRepository
{
    private readonly ConfigurationDbContext _context;
    private readonly IMapper _mapper;

    public ConfigurationRepository(ConfigurationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ConfigurationEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Configurations
            .Where(c => c.Id == id && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ConfigurationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Configurations
            .AsNoTracking()
            .Where(c => c.Id == id && !c.IsDeleted)
            .ProjectTo<ConfigurationDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<ConfigurationDto>> ListAsync(ListConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Configurations.AsNoTracking().Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.MarketCode))
            query = query.Where(c => c.MarketCode == request.MarketCode);
        if (!string.IsNullOrWhiteSpace(request.ServiceCode))
            query = query.Where(c => c.ServiceCode == request.ServiceCode);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                (c.Key != null && c.Key.ToLower().Contains(term)) ||
                (c.Value != null && c.Value.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sortBy = request.SortBy?.ToLowerInvariant() ?? "key";
        var ascending = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = sortBy switch
        {
            "marketcode" => ascending ? query.OrderBy(c => c.MarketCode) : query.OrderByDescending(c => c.MarketCode),
            "servicecode" => ascending ? query.OrderBy(c => c.ServiceCode) : query.OrderByDescending(c => c.ServiceCode),
            "value" => ascending ? query.OrderBy(c => c.Value) : query.OrderByDescending(c => c.Value),
            "createdat" => ascending ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
            "updatedat" => ascending ? query.OrderBy(c => c.UpdatedAt) : query.OrderByDescending(c => c.UpdatedAt),
            _ => ascending ? query.OrderBy(c => c.Key) : query.OrderByDescending(c => c.Key)
        };

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ConfigurationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResponse<ConfigurationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ConfigurationDto?> GetByScopeAndKeyAsync(string? marketCode, string? serviceCode, string key, CancellationToken cancellationToken = default)
    {
        return await _context.Configurations
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.MarketCode == marketCode && c.ServiceCode == serviceCode && c.Key == key)
            .ProjectTo<ConfigurationDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ConfigurationEntity>> GetConfigurationsAsync(ConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var marketCode = string.IsNullOrWhiteSpace(request.MarketCode)
            ? null
            : request.MarketCode;

        var marketArea = string.IsNullOrWhiteSpace(request.MarketArea)
            ? null
            : request.MarketArea;

        var serviceCode = string.IsNullOrWhiteSpace(request.ServiceCode)
            ? null
            : request.ServiceCode;

        var rows = await _context.Configurations
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Where(x => x.MarketCode == null || x.MarketCode == marketCode)
            .Where(x => x.ServiceCode == null || x.ServiceCode == serviceCode)
            .ToListAsync(cancellationToken);

        // If you later add a real MarketArea column, handle it here.
        // Right now do NOT compare MarketCode to MarketArea.

        var ordered = rows
            .OrderBy(x => x.Key)
            .ThenBy(x => GetSpecificity(x, marketCode, serviceCode))
            .ThenBy(x => x.UpdatedAt)
            .ToList();

        return ordered;
    }

    private static int GetSpecificity(
        ConfigurationEntity entity,
        string? marketCode,
        string? serviceCode)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(entity.MarketCode) &&
            string.Equals(entity.MarketCode, marketCode, StringComparison.OrdinalIgnoreCase))
        {
            score += 1;
        }

        if (!string.IsNullOrWhiteSpace(entity.ServiceCode) &&
            string.Equals(entity.ServiceCode, serviceCode, StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        return score;
    }

    public void Add(ConfigurationEntity entity)
    {
        _context.Configurations.Add(entity);
    }

    public void Update(ConfigurationEntity entity)
    {
        _context.Configurations.Update(entity);
    }

    public void SoftDelete(ConfigurationEntity entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _context.Configurations.Update(entity);
    }
}
