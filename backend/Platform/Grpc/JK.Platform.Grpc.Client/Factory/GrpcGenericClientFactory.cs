using Grpc.Core;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Grpc.Client.Factory;

[Injectable(lifetime: ServiceLifetime.Singleton)]
public sealed class GrpcGenericClientFactory(IGrpcChannelFactory channelFactory) : IGrpcGenericClientFactory
{
    public IGrpcGenericClient GetClient(string channelUrl)
    {
        return new GrpcGenericClient(channelFactory.GetInvoker(channelUrl));
    }

    private sealed class GrpcGenericClient : IGrpcGenericClient
    {
        private readonly CallInvoker _invoker;

        public GrpcGenericClient(CallInvoker invoker)
        {
            _invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
        }

        public async Task<TResponse> CallAsync<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            TRequest request,
            CallOptions options = default)
            where TRequest : class
            where TResponse : class
        {
            ArgumentNullException.ThrowIfNull(method);
            ArgumentNullException.ThrowIfNull(request);

            var call = _invoker.AsyncUnaryCall(method, null, options, request);
            return await call.ResponseAsync.ConfigureAwait(false);
        }

        public async Task<byte[]> CallRawAsync(
            string serviceName,
            string methodName,
            byte[] request,
            CallOptions options = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
            ArgumentException.ThrowIfNullOrWhiteSpace(methodName);
            ArgumentNullException.ThrowIfNull(request);

            var method = new Method<byte[], byte[]>(
                MethodType.Unary,
                serviceName,
                methodName,
                Marshallers.Create(static r => r, static r => r),
                Marshallers.Create(static r => r, static r => r));

            var call = _invoker.AsyncUnaryCall(method, null, options, request);
            return await call.ResponseAsync.ConfigureAwait(false);
        }
    }
}