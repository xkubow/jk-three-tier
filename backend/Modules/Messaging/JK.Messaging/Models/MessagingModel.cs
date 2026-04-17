using JK.Platform.Persistence.EfCore;

namespace JK.Messaging.Models;

public class MessagingModel : ModelBase<Guid>
{
    public Guid ThreadId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
