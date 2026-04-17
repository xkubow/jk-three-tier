using JK.Messaging.Contracts;
using JK.Messaging.Database.Entities;
using JK.Messaging.Models;
using JK.Platform.Persistence.EfCore;

namespace JK.Messaging.Database.Repositories;

public interface IMessagingRepository : IRepository<MessagingModel, Guid>
{
    Task<MessagingEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResponse<MessagingModel>> ListAsync(ListMessagingRequest request,
        CancellationToken cancellationToken = default);
}
