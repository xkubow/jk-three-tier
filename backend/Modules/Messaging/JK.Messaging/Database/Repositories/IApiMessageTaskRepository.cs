using JK.Messaging.Database.Entities;
using JK.Messaging.Models;
using JK.Platform.Persistence.EfCore;

namespace JK.Messaging.Database.Repositories;

public interface IApiMessageTaskRepository : IRepository<ApiMessageTaskModel, string>
{
    Task<ApiMessageTaskEntity?> GetEntityByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(ApiMessageTaskEntity entity, CancellationToken cancellationToken = default);
    Task UpdateEntityAsync(ApiMessageTaskEntity entity, CancellationToken cancellationToken = default);
}