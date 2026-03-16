using JK.Configuration.Database.Repositories;

namespace JK.Configuration.Database;

public interface IUnitOfWork : IDisposable
{
    IConfigurationRepository Configurations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
