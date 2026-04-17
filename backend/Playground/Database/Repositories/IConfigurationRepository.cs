using Backend.Models;

namespace JK.Playground.Database.Repositories;

public interface IConfigurationRepository
{
    Task<IEnumerable<ConfigurationModel>> GetAllAsync();
    Task<ConfigurationModel?> GetByKeyAsync(string key);
}