using Backend.Models;
using Backend.Stores;

namespace Backend.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationStore _store;

    public ConfigurationService(IConfigurationStore store)
    {
        _store = store;
    }

    public async Task<IEnumerable<ConfigurationModel>> GetAllConfigurationsAsync()
    {
        return await _store.GetAllAsync();
    }

    public async Task<ConfigurationModel?> GetConfigurationByKeyAsync(string key)
    {
        return await _store.GetByKeyAsync(key);
    }
}