using Backend.Database.Repositories;

namespace Backend.Database;

public interface IUnitOfWork : IDisposable
{
    IConfigurationRepository Configurations { get; }
    Task<int> SaveChangesAsync();
}
