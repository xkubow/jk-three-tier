using AutoMapper;
using JK.Platform.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

public abstract class BaseRepository<TModel, TEntity, TKey> : IRepository<TModel, TKey>
    where TModel : ModelBase<TKey>
    where TEntity : EntityBase<TKey>
{
    protected readonly DbContext Context;
    protected readonly DbSet<TEntity> DbSet;
    protected readonly IMapper Mapper;

    protected BaseRepository(DbContext context, IMapper mapper)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
        Mapper = mapper;
    }

    protected virtual IQueryable<TEntity> Query => DbSet.AsQueryable();

    public virtual async Task<TModel?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await Query.FirstOrDefaultAsync(x => x.Id!.Equals(id), cancellationToken);
        return entity is null ? null : Mapper.Map<TModel>(entity);
    }

    public virtual async Task<List<TModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Query.ToListAsync(cancellationToken);
        return Mapper.Map<List<TModel>>(entities);
    }

    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await Query.AnyAsync(x => x.Id!.Equals(id), cancellationToken);
    }

    public virtual async Task AddAsync(TModel model, CancellationToken cancellationToken = default)
    {
        var entity = Mapper.Map<TEntity>(model);

        if (entity is ICreatedOnEntity createdOnEntity)
        {
            createdOnEntity.CreatedAt = DateTime.UtcNow;
        }

        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
    {
        var entities = Mapper.Map<List<TEntity>>(models);

        foreach (var entity in entities)
        {
            if (entity is ICreatedOnEntity createdOnEntity)
            {
                createdOnEntity.CreatedAt = DateTime.UtcNow;
            }
        }

        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual async Task UpdateAsync(TModel model, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FirstOrDefaultAsync(
            x => x.Id!.Equals(model.Id),
            cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException($"{typeof(TEntity).Name} with id '{model.Id}' was not found.");
        }

        Mapper.Map(model, entity);

        if (entity is IUpdatedOnEntity updatedOnEntity)
        {
            updatedOnEntity.UpdatedAt = DateTime.UtcNow;
        }

        DbSet.Update(entity);
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FirstOrDefaultAsync(
            x => x.Id!.Equals(id),
            cancellationToken);

        if (entity is null)
        {
            return;
        }

        if (entity is ISoftDeleteEntity softDeleteEntity)
        {
            softDeleteEntity.IsDeleted = true;
            softDeleteEntity.DeletedAt = DateTime.UtcNow;
            DbSet.Update(entity);
        }
        else
        {
            DbSet.Remove(entity);
        }
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}