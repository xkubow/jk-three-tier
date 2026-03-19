using AutoMapper;
using JK.Configuration.Contracts;
using JK.Configuration.Database;
using JK.Configuration.Database.Entities;
using JK.Configuration.Database.Repositories;
using JK.Configuration.Models;
using JK.Platform.Core.DependencyInjection.Attributes;

namespace JK.Configuration.Services;

[Injectable]
public class ConfigurationService : IConfigurationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ConfigurationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ConfigurationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
    }

    public async Task<PagedResponse<ConfigurationDto>> ListAsync(ListConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Configurations.ListAsync(request, cancellationToken);
    }

    public async Task<ConfigurationDto> CreateAsync(CreateConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Configurations.GetByScopeAndKeyAsync(
            request.MarketCode, request.ServiceCode, request.Key, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException(
                $"Configuration already exists for MarketCode={request.MarketCode}, ServiceCode={request.ServiceCode}, Key={request.Key}.");

        var entity = new ConfigurationEntity
        {
            Id = Guid.NewGuid(),
            MarketCode = string.IsNullOrWhiteSpace(request.MarketCode) ? null : request.MarketCode.Trim(),
            ServiceCode = string.IsNullOrWhiteSpace(request.ServiceCode) ? null : request.ServiceCode.Trim(),
            Key = request.Key.Trim(),
            Value = request.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy,
            UpdatedBy = request.CreatedBy,
            IsDeleted = false
        };
        _unitOfWork.Configurations.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _unitOfWork.Configurations.GetByIdAsync(entity.Id, cancellationToken);
        return dto!;
    }

    public async Task<ConfigurationDto?> UpdateAsync(Guid id, UpdateConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Configurations.GetEntityByIdAsync(id, cancellationToken);
        if (entity == null) return null;

        entity.Value = request.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = request.UpdatedBy;
        _unitOfWork.Configurations.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Configurations.GetEntityByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _unitOfWork.Configurations.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<ConfigurationDto>> GetConfigurationsAsync(ConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.Configurations.GetConfigurationsAsync(request, cancellationToken);
        return _mapper.Map<List<ConfigurationDto>>(entities);
    }
}
