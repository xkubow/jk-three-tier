using AutoMapper;
using JK.Playground.Database;
using JK.Playground.Database.Repositories;

namespace Backend.Database;

public class UnitOfWork : IUnitOfWork
{
    private readonly ConfigDbContext _context;
    private readonly IMapper _mapper;
    private IConfigurationRepository? _configurationRepository;

    public UnitOfWork(ConfigDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IConfigurationRepository Configurations => _configurationRepository ??= new ConfigurationRepository(_context, _mapper);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
