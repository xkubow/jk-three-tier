using JK.Configuration.Contracts;
using JK.Configuration.Database.Entities;

namespace JK.Configuration.Database.Repositories;

public interface IConfigurationRepository
{
    Task<ConfigurationEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConfigurationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ConfigurationDto>> ListAsync(ListConfigurationRequest request, CancellationToken cancellationToken = default);
    Task<ConfigurationDto?> GetByScopeAndKeyAsync(string? marketCode, string? serviceCode, string key, CancellationToken cancellationToken = default);
    void Add(ConfigurationEntity entity);
    void Update(ConfigurationEntity entity);
    void SoftDelete(ConfigurationEntity entity);
}
