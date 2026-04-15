namespace JK.Messaging.Contracts;

public class EchoViaOrleansRequest
{
    public Guid ThreadId { get; set; }

    public string SenderId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
