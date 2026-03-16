using Backend.Models;

namespace Backend.Stores;

public interface IConfigurationStore
{
    Task<IEnumerable<ConfigurationModel>> GetAllAsync();
    Task<ConfigurationModel?> GetByKeyAsync(string key);
}