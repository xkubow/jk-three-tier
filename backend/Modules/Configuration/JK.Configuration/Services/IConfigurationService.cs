using JK.Configuration.Contracts;

namespace JK.Configuration.Services;

public interface IConfigurationService
{
    Task<ConfigurationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ConfigurationDto>> ListAsync(ListConfigurationRequest request, CancellationToken cancellationToken = default);
    Task<ConfigurationDto> CreateAsync(CreateConfigurationRequest request, CancellationToken cancellationToken = default);
    Task<ConfigurationDto?> UpdateAsync(Guid id, UpdateConfigurationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
