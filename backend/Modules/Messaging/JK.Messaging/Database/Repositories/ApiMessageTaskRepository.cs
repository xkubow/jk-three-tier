using AutoMapper;
using JK.Messaging.Contracts.Enums;
using JK.Messaging.Database.Entities;
using JK.Messaging.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Messaging.Database.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class ApiMessageTaskRepository
    : BaseRepository<ApiMessageTaskModel, ApiMessageTaskEntity, string>, IApiMessageTaskRepository
{
    public ApiMessageTaskRepository(MessagingDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<ApiMessageTaskEntity?> GetEntityByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(ApiMessageTaskEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public Task UpdateEntityAsync(ApiMessageTaskEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }
}