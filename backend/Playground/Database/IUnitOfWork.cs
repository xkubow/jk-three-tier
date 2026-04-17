using JK.Playground.Database.Repositories;

namespace JK.Playground.Database;

public interface IUnitOfWork : IDisposable
{
    IConfigurationRepository Configurations { get; }
    Task<int> SaveChangesAsync();
}
