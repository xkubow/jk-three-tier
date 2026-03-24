using AutoMapper;
using JK.Configuration.Database.Repositories;
using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Configuration.Database;

[Injectable(ServiceLifetime.Scoped)]
public class UnitOfWork : UnitOfWork<ConfigurationDbContext>, IUnitOfWork
{
    private readonly IMapper _mapper;
    private IConfigurationRepository? _configurationRepository;

    public UnitOfWork(ConfigurationDbContext context, IMapper mapper) : base(context)
    {
        _mapper = mapper;
    }

    public IConfigurationRepository Configurations =>
        _configurationRepository ??= new ConfigurationRepository(Context, _mapper);
}
