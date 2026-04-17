using Orleans;

namespace JK.Messaging.Grains;

public sealed class ConversationGrain : Grain, IConversationGrain
{
    public Task<string> EchoAsync(string senderId, string content)
    {
        var threadId = this.GetPrimaryKey();
        return Task.FromResult($"[{threadId:N}] {senderId}: {content}");
    }
}
