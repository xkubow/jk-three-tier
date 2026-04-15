using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JK.Platform.Persistence.EfCore;

namespace JK.Messaging.Database.Entities;

[Table("Messaging")]
public class MessagingEntity : EntityBase<Guid>, ICreatedOnEntity, IUpdatedOnEntity
{
    public Guid ThreadId { get; set; }

    public Guid SenderId { get; set; }

    [Required]
    [MaxLength(8000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
