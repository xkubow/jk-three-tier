using AutoMapper;
using JK.Configuration.Contracts;
using JK.Configuration.Database;
using JK.Configuration.Database.Entities;
using JK.Configuration.Database.Repositories;
using JK.Configuration.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Configuration.Services;

[Injectable(ServiceLifetime.Scoped)]
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
        var configurationModel = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<ConfigurationDto>(configurationModel);
    }

    public async Task<PagedResponse<ConfigurationDto>> ListAsync(ListConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var pagedResponse = await _unitOfWork.Configurations.ListAsync(request, cancellationToken);
        return _mapper.Map<PagedResponse<ConfigurationDto>>(pagedResponse);
    }

    public async Task<ConfigurationDto> CreateAsync(CreateConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Configurations.GetByScopeAndKeyAsync(
            request.MarketCode, request.ServiceCode, request.Key, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException(
                $"Configuration already exists for MarketCode={request.MarketCode}, ServiceCode={request.ServiceCode}, Key={request.Key}.");

        var model = new ConfigurationModel()
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
        };
        await _unitOfWork.Configurations.AddAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var insertedModel = await _unitOfWork.Configurations.GetByIdAsync(model.Id, cancellationToken);
        return _mapper.Map<ConfigurationDto>(insertedModel);
    }

    public async Task<ConfigurationDto?> UpdateAsync(Guid id, UpdateConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var model = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (model == null) return null;

        model.Value = request.Value;
        model.UpdatedAt = DateTime.UtcNow;
        model.UpdatedBy = request.UpdatedBy;
        await _unitOfWork.Configurations.UpdateAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedModel = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<ConfigurationDto>(updatedModel);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        await _unitOfWork.Configurations.DeleteAsync(id,cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<ConfigurationDto>> GetConfigurationsAsync(ConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var configurationModels = await _unitOfWork.Configurations.GetConfigurationsAsync(request, cancellationToken);
        return _mapper.Map<List<ConfigurationDto>>(configurationModels);
    }
}
