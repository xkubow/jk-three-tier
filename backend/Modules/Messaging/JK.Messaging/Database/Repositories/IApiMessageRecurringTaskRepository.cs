using JK.Platform.Persistence.EfCore;
using ApiMessageRecurringTaskModel = JK.Messaging.Models.ApiMessageRecurringTaskModel;

namespace JK.Messaging.Database.Repositories;

public interface IApiMessageRecurringTaskRepository : IRepository<ApiMessageRecurringTaskModel, string>
{
    Task<IEnumerable<ApiMessageRecurringTaskModel>> GetDueTasksAsync(DateTime now);
    Task<IReadOnlyCollection<ApiMessageRecurringTaskModel>> GetEnabledAsync();
    Task<ApiMessageRecurringTaskModel> GetFirstOrDefaultAsync(string taskName);
}