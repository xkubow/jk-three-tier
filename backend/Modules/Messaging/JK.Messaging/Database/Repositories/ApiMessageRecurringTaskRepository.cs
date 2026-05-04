using AutoMapper;
using JK.Messaging.Database.Entities;
using JK.Messaging.Models;
using Microsoft.EntityFrameworkCore;

namespace JK.Messaging.Database.Repositories;

public class ApiMessageRecurringTaskRepository : BaseRepository<ApiMessageRecurringTaskModel, ApiMessageRecurringTaskEntity, string>, IApiMessageRecurringTaskRepository
{
    public ApiMessageRecurringTaskRepository(DbContext context, IMapper mapper) : base(context, mapper)
    {

    }

    public async Task<IEnumerable<ApiMessageRecurringTaskModel>> GetDueTasksAsync(DateTime now)
    {
        var dueTasks = await DbSet.Where(x =>
            x.IsEnabled &&
            x.NextRunAtUtc != null &&
            x.NextRunAtUtc <= now)
            .AsNoTracking()
            .ToListAsync();

        return Mapper.Map<IEnumerable<ApiMessageRecurringTaskModel>>(dueTasks);
    }

    public async Task<IReadOnlyCollection<ApiMessageRecurringTaskModel>> GetEnabledAsync()
    {
        var entities = await DbSet.Where(x => x.IsEnabled).AsNoTracking().ToListAsync();

        return Mapper.Map<IReadOnlyCollection<ApiMessageRecurringTaskModel>>(entities);
    }

    public async Task<ApiMessageRecurringTaskModel> GetFirstOrDefaultAsync(string taskName)
    {
        var entitie = await DbSet.FirstOrDefaultAsync(x => x.TaskName == taskName);
        return Mapper.Map<ApiMessageRecurringTaskModel>(entitie);
    }
}