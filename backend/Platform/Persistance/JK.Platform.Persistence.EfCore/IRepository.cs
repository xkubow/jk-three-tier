namespace JK.Platform.Persistence.EfCore;

public interface IRepository<TModel, TKey>
    where TModel : ModelBase<TKey>
{
    Task<TModel?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<List<TModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TModel model, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TModel> models, CancellationToken cancellationToken = default);
    Task UpdateAsync(TModel model, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}