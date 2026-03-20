using JK.Configuration.Contracts;
using JK.Configuration.Database.Entities;
using JK.Configuration.Models;
using JK.Platform.Persistence.EfCore;

namespace JK.Configuration.Database.Repositories;

public interface IConfigurationRepository: IRepository<ConfigurationModel, Guid>
{
    Task<PagedResponse<ConfigurationModel>> ListAsync(ListConfigurationRequest request, CancellationToken cancellationToken = default);
    Task<ConfigurationModel?> GetByScopeAndKeyAsync(string? marketCode, string? serviceCode, string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ConfigurationModel>> GetConfigurationsAsync(ConfigurationRequest request, CancellationToken cancellationToken = default);
}
