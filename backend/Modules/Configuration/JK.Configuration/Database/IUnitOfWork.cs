using JK.Configuration.Database.Repositories;

namespace JK.Configuration.Database;

public interface IUnitOfWork : Platform.Core.Persistence.IUnitOfWork
{
    IConfigurationRepository Configurations { get; }
}
