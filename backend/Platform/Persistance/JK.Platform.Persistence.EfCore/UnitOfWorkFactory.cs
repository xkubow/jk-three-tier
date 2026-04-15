using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Persistence.EfCore;

[Injectable(ServiceLifetime.Scoped)]
public class UnitOfWorkFactory<TDbContext> : IUnitOfWorkFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    public UnitOfWorkFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IUnitOfWork<TDbContext> Create()
    {
        return _serviceProvider.GetRequiredService<IUnitOfWork<TDbContext>>();
    }
}