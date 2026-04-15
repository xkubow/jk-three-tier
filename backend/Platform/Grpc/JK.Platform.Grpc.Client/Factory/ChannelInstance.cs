using Grpc.Core;
using Grpc.Net.Client;

namespace JK.Platform.Grpc.Client.Factory;

internal sealed class ChannelInstance : IDisposable
{
    public GrpcChannel Channel { get; }
    public CallInvoker CallInvoker { get; }
    public string ChannelUrl { get; }
    public long Generation { get; }

    public ChannelInstance(
        GrpcChannel channel,
        string channelUrl,
        CallInvoker callInvoker,
        long generation)
    {
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        ChannelUrl = string.IsNullOrWhiteSpace(channelUrl)
            ? throw new ArgumentException("Channel URL cannot be null or empty.", nameof(channelUrl))
            : channelUrl;
        CallInvoker = callInvoker ?? throw new ArgumentNullException(nameof(callInvoker));
        Generation = generation;
    }

    public void Dispose()
    {
        Channel.Dispose();
    }
}