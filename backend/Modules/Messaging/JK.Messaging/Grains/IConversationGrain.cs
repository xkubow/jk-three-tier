using Orleans;

namespace JK.Messaging.Grains;

public interface IConversationGrain : IGrainWithGuidKey
{
    Task<string> EchoAsync(string senderId, string content);
}
