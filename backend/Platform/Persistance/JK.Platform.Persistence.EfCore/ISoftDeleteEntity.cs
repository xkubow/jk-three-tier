namespace JK.Platform.Persistence.EfCore;

public interface ISoftDeleteEntity
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}