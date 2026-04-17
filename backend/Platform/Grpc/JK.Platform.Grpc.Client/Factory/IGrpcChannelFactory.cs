using Grpc.Core;
using Grpc.Net.Client;

namespace JK.Platform.Grpc.Client.Factory;

public interface IGrpcChannelFactory
{
    CallInvoker GetInvoker(string channelUrl);
    GrpcChannel GetChannel(string channelUrl);
    void Invalidate(string channelUrl);
}