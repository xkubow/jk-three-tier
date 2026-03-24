namespace JK.Platform.Persistence.EfCore;

public abstract class ModelBase<TKey>
{
    public TKey Id { get; set; } = default!;
}