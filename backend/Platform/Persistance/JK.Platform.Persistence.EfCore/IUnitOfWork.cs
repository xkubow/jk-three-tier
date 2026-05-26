using Microsoft.EntityFrameworkCore;

namespace JK.Platform.Persistence.EfCore;

public interface IUnitOfWork<TDbContext> : IDisposable
    where TDbContext : DbContextBase
{
    TRepository GetRepository<TRepository>() where TRepository : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}