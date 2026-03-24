using Backend.Models;

namespace Backend.Database.Repositories;

public interface IConfigurationRepository
{
    Task<IEnumerable<ConfigurationModel>> GetAllAsync();
    Task<ConfigurationModel?> GetByKeyAsync(string key);
}