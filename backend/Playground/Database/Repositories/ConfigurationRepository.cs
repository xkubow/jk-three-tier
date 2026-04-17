using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Database;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace JK.Playground.Database.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly ConfigDbContext _context;
    private readonly IMapper _mapper;

    public ConfigurationRepository(ConfigDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ConfigurationModel>> GetAllAsync()
    {
        return await _context.Configuration
            .ProjectTo<ConfigurationModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ConfigurationModel?> GetByKeyAsync(string key)
    {
        return await _context.Configuration
            .Where(c => c.Key == key)
            .ProjectTo<ConfigurationModel>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
}