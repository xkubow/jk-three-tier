using Grpc.Core;

namespace JK.Platform.Grpc.Client.Factory;

public interface IGrpcGenericClient
{
    Task<TResponse> CallAsync<TRequest, TResponse>(
        Method<TRequest, TResponse> method,
        TRequest request,
        CallOptions options = default)
        where TRequest : class
        where TResponse : class;

    Task<byte[]> CallRawAsync(
        string serviceName,
        string methodName,
        byte[] request,
        CallOptions options = default);
}
