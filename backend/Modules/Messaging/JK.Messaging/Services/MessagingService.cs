using AutoMapper;
using JK.Messaging.Configurations;
using JK.Messaging.Contracts;
using JK.Messaging.Database.Repositories;
using JK.Messaging.Database;
using JK.Messaging.Grains;
using JK.Messaging.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Persistence.EfCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JK.Messaging.Services;

[Injectable(ServiceLifetime.Scoped)]
public class MessagingService : IMessagingService
{
    private readonly IMapper _mapper;
    private readonly IOptionsSnapshot<MessagingConfiguration> _configuration;
    private readonly IGrainFactory _grainFactory;
    private readonly IUnitOfWork<MessagingDbContext> _unitOfWork;

    public MessagingService(
        IMapper mapper,
        IOptionsSnapshot<MessagingConfiguration> configuration,
        IGrainFactory grainFactory, IUnitOfWorkFactory<MessagingDbContext> unitOfWorkFactory)
    {
        _mapper = mapper;
        _configuration = configuration;
        _grainFactory = grainFactory;
        _unitOfWork = unitOfWorkFactory.Create();
    }

    public async Task<MessagingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _unitOfWork.GetRepository<IMessagingRepository>().GetByIdAsync(id, cancellationToken);
        return _mapper.Map<MessagingDto>(model);
    }

    public async Task<PagedResponse<MessagingDto>> ListAsync(ListMessagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagedResponse = await _unitOfWork.GetRepository<IMessagingRepository>().ListAsync(request, cancellationToken);
        return _mapper.Map<PagedResponse<MessagingDto>>(pagedResponse);
    }

    public async Task<MessagingDto> CreateAsync(CreateMessagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = new MessagingModel
        {
            Id = Guid.NewGuid(),
            ThreadId = request.ThreadId,
            SenderId = request.SenderId,
            Content = request.Content.Trim(),
            Status = "Sent",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };



        await _unitOfWork.GetRepository<IMessagingRepository>().AddAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var insertedModel = await _unitOfWork.GetRepository<IMessagingRepository>().GetByIdAsync(model.Id, cancellationToken);
        return _mapper.Map<MessagingDto>(insertedModel);
    }

    public async Task<MessagingDto?> UpdateAsync(Guid id, UpdateMessagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = await _unitOfWork.GetRepository<IMessagingRepository>().GetByIdAsync(id, cancellationToken);
        if (model == null) return null;

        model.Status = request.Status.Trim();
        model.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.GetRepository<IMessagingRepository>().UpdateAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedModel = await _unitOfWork.GetRepository<IMessagingRepository>().GetByIdAsync(id, cancellationToken);
        return _mapper.Map<MessagingDto>(updatedModel);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.GetRepository<IMessagingRepository>().ExistsAsync(id, cancellationToken);
        if (!exists) return false;

        await _unitOfWork.GetRepository<IMessagingRepository>().DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<string> EchoViaOrleansAsync(EchoViaOrleansRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_configuration.Value.EnableOrleansEcho)
            return "Orleans echo is disabled.";

        var grain = _grainFactory.GetGrain<IConversationGrain>(request.ThreadId);
        return await grain.EchoAsync(request.SenderId, request.Content);
    }

    public async Task<bool> RegisterApiMessageAsync(RegisterApiMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var grainId = Guid.NewGuid().GetHashCode().ToString("X");
        var grain = _grainFactory.GetGrain<IApiMessageGrain>(grainId);
        return await grain.Register(request.Cron);
    }

    public async Task SendApiMessageAsync(SendApiMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var grainId = request.GrainId ?? Guid.NewGuid().GetHashCode().ToString("X");
        var grain = _grainFactory.GetGrain<IApiMessageGrain>(grainId);
        await grain.SendApiMessage(request.Url);
    }
}
