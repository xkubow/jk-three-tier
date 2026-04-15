namespace JK.Platform.Grpc.Client.Factory;

public interface IGrpcGenericClientFactory
{
    IGrpcGenericClient GetClient(string channelUrl);
}
