using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JK.Platform.Grpc.Client.Factory;

using JK.Platform.Grpc.Client;

public sealed class GrpcClientFactory<TClient> : IGrpcClientFactory<TClient>
    where TClient : ClientBase<TClient>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GrpcClientFactory<TClient>> _logger;
    private readonly IOptionsMonitor<GrpcClientConfiguration> _configuration;

    private readonly Dictionary<string, GrpcChannel> _channels = new();
    private static readonly object _lock = new();

    public GrpcClientFactory(
        IServiceProvider serviceProvider,
        ILogger<GrpcClientFactory<TClient>> logger,
        IOptionsMonitor<GrpcClientConfiguration> configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public TClient GetClient(string channelUrl)
    {
        if (string.IsNullOrWhiteSpace(channelUrl))
        {
            throw new InvalidOperationException("gRPC URL cannot be null or empty.");
        }

        var channel = GetOrCreateChannel(channelUrl);

        var client = (TClient?)Activator.CreateInstance(typeof(TClient), channel);

        if (client is null)
        {
            throw new InvalidOperationException($"Failed to create gRPC client {typeof(TClient)}");
        }

        return client;
    }

    private GrpcChannel GetOrCreateChannel(string url)
    {
        lock (_lock)
        {
            if (_channels.TryGetValue(url, out var existing))
            {
                if (existing.State != ConnectivityState.Shutdown &&
                    existing.State != ConnectivityState.TransientFailure)
                {
                    return existing;
                }

                _logger.LogWarning("Recreating gRPC channel for {Url}", url);
                existing.Dispose();
            }

            var channel = CreateChannel(url);
            _channels[url] = channel;

            return channel;
        }
    }

    private GrpcChannel CreateChannel(string url)
    {
        var config = _configuration.CurrentValue;

        var dnsUrl = config.UseSecureSslChannel
            ? url.Replace("https://", "dns:///")
            : url.Replace("http://", "dns:///");

        var retryPolicy = new RetryPolicy
        {
            MaxAttempts = config.RetryMaxAttempts,
            InitialBackoff = TimeSpan.FromSeconds(2),
            MaxBackoff = TimeSpan.FromSeconds(10),
            BackoffMultiplier = 1.5,
            RetryableStatusCodes = { StatusCode.Unavailable }
        };

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(config.PooledConnectionLifetimeMinutes),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(config.PooledConnectionIdleTimeoutSeconds),
            MaxConnectionsPerServer = config.MaxConnectionsPerServer,
            EnableMultipleHttp2Connections = true
        };

        if (!config.UseSecureSslChannel)
        {
            handler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
        }

        var channel = GrpcChannel.ForAddress(dnsUrl, new GrpcChannelOptions
        {
            Credentials = config.UseSecureSslChannel
                ? ChannelCredentials.SecureSsl
                : ChannelCredentials.Insecure,

            HttpHandler = handler,

            ServiceConfig = new ServiceConfig
            {
                MethodConfigs = { new MethodConfig { Names = { MethodName.Default }, RetryPolicy = retryPolicy } },
                LoadBalancingConfigs = { new RoundRobinConfig() }
            },

            ServiceProvider = _serviceProvider
        });

        return channel;
    }
}