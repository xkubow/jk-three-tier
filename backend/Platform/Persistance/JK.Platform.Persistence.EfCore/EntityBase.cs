using System.ComponentModel.DataAnnotations;

namespace JK.Platform.Persistence.EfCore;

public abstract class EntityBase<TKey>
{
    [Key]
    public TKey Id { get; set; } = default!;
}