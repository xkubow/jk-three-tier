using Backend.Models;

namespace Backend.Services;

public interface IConfigurationService
{
    Task<IEnumerable<ConfigurationModel>> GetAllConfigurationsAsync();
    Task<ConfigurationModel?> GetConfigurationByKeyAsync(string key);
}