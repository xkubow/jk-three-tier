namespace JK.Messaging.Contracts;

public class CreateMessagingRequest
{
    public Guid ThreadId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;
}
