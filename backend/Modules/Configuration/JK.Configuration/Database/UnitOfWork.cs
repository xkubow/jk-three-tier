using AutoMapper;
using JK.Configuration.Database.Repositories;
using JK.Platform.Core.DependencyInjection.Attributes;

namespace JK.Configuration.Database;

[Injectable]
public class UnitOfWork : IUnitOfWork
{
    private readonly ConfigurationDbContext _context;
    private readonly IMapper _mapper;
    private IConfigurationRepository? _configurationRepository;

    public UnitOfWork(ConfigurationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IConfigurationRepository Configurations =>
        _configurationRepository ??= new ConfigurationRepository(_context, _mapper);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
