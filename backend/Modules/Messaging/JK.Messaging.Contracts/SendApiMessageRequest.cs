namespace JK.Messaging.Contracts;

public class SendApiMessageRequest
{
    public string Url { get; set; } = string.Empty;
    public string? GrainId { get; set; }
}
