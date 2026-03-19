namespace JK.Platform.Persistence.EfCore;

public interface IUpdatedOnEntity
{
    DateTime? UpdatedAt { get; set; }
}