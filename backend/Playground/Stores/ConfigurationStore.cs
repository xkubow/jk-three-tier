using Backend.Database;
using Backend.Models;
using JK.Playground.Database;

namespace Backend.Stores;

public class ConfigurationStore : IConfigurationStore
{
    private readonly IUnitOfWork _uow;

    public ConfigurationStore(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<ConfigurationModel>> GetAllAsync()
    {
        return await _uow.Configurations.GetAllAsync();
    }

    public async Task<ConfigurationModel?> GetByKeyAsync(string key)
    {
        return await _uow.Configurations.GetByKeyAsync(key);
    }
}