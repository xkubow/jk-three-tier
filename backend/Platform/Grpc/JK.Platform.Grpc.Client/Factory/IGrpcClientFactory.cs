using Grpc.Core;

namespace JK.Platform.Grpc.Client.Factory;

public interface IGrpcClientFactory<TGrpcClient> where TGrpcClient : ClientBase<TGrpcClient>
{
    TGrpcClient GetClient(string channelUrl);
}