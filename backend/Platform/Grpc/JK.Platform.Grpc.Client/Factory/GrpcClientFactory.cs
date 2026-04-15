using Grpc.Core;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Grpc.Client.Factory;

[Injectable(lifetime: ServiceLifetime.Singleton)]
public sealed class GrpcClientFactory<TClient> : IGrpcClientFactory<TClient>
    where TClient : ClientBase<TClient>
{
    private readonly IGrpcChannelFactory _channelFactory;
    private readonly ILogger<GrpcClientFactory<TClient>> _logger;

    public GrpcClientFactory(
        IGrpcChannelFactory channelFactory,
        ILogger<GrpcClientFactory<TClient>> logger)
    {
        _channelFactory = channelFactory;
        _logger = logger;
    }

    public TClient GetClient(string channelUrl)
    {
        if (string.IsNullOrWhiteSpace(channelUrl))
        {
            throw new InvalidOperationException("gRPC URL cannot be null or empty.");
        }

        var invoker = _channelFactory.GetInvoker(channelUrl);

        var client = (TClient?)Activator.CreateInstance(typeof(TClient), invoker);
        if (client is null)
        {
            _logger.LogError(
                "Failed to create gRPC client of type {ClientType} for {ChannelUrl}",
                typeof(TClient).FullName,
                channelUrl);

            throw new InvalidOperationException(
                $"Failed to create gRPC client '{typeof(TClient).FullName}'.");
        }

        return client;
    }
}