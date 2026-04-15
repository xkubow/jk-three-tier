using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace JK.Platform.Grpc.Client.Factory;

[Injectable(lifetime: ServiceLifetime.Singleton)]
public sealed class GrpcChannelFactory : IGrpcChannelFactory, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GrpcChannelFactory> _logger;
    private readonly IOptionsMonitor<GrpcClientConfiguration> _configuration;
    private readonly IReadOnlyCollection<Interceptor> _interceptors;

    private readonly ConcurrentDictionary<string, ChannelInstance> _channels = new();
    private readonly ConcurrentDictionary<string, long> _generations = new();
    private readonly object _sync = new();

    private bool _disposed;

    public GrpcChannelFactory(
        IServiceProvider serviceProvider,
        ILogger<GrpcChannelFactory> logger,
        IOptionsMonitor<GrpcClientConfiguration> configuration,
        IEnumerable<Interceptor> interceptors)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _interceptors = interceptors.ToArray();

        _configuration.OnChange(_ =>
        {
            _logger.LogInformation("gRPC client configuration changed. Existing channels will remain active until recreated.");
        });
    }

    public CallInvoker GetInvoker(string channelUrl)
    {
        ThrowIfDisposed();
        return GetOrCreateChannelInstance(channelUrl).CallInvoker;
    }

    public GrpcChannel GetChannel(string channelUrl)
    {
        ThrowIfDisposed();
        return GetOrCreateChannelInstance(channelUrl).Channel;
    }

    public void Invalidate(string channelUrl)
    {
        if (string.IsNullOrWhiteSpace(channelUrl))
        {
            return;
        }

        ThrowIfDisposed();

        if (_channels.TryRemove(channelUrl, out var instance))
        {
            _logger.LogWarning(
                "Invalidating gRPC channel for {ChannelUrl}. Generation: {Generation}",
                channelUrl,
                instance.Generation);

            instance.Dispose();
        }
    }

    private ChannelInstance GetOrCreateChannelInstance(string channelUrl)
    {
        if (string.IsNullOrWhiteSpace(channelUrl))
        {
            throw new InvalidOperationException("gRPC URL cannot be null or empty.");
        }

        if (_channels.TryGetValue(channelUrl, out var cached))
        {
            if (cached.Channel.State != ConnectivityState.Shutdown)
            {
                return cached;
            }

            _logger.LogWarning(
                "Cached gRPC channel for {ChannelUrl} is shutdown. Recreating. Generation: {Generation}",
                channelUrl,
                cached.Generation);
        }

        lock (_sync)
        {
            if (_channels.TryGetValue(channelUrl, out cached))
            {
                if (cached.Channel.State != ConnectivityState.Shutdown)
                {
                    return cached;
                }

                _channels.TryRemove(channelUrl, out _);
                cached.Dispose();
            }

            var nextGeneration = _generations.AddOrUpdate(channelUrl, 1, (_, current) => current + 1);
            var instance = CreateChannelInstance(channelUrl, nextGeneration);

            _channels[channelUrl] = instance;
            return instance;
        }
    }

    private ChannelInstance CreateChannelInstance(string rawUrl, long generation)
    {
        var config = _configuration.CurrentValue;
        var uri = ParseAndValidateUri(rawUrl);

        var useSecureChannel = ResolveSecureMode(uri, config);

        _logger.LogInformation(
            "Creating gRPC channel for {ChannelUrl}. Generation: {Generation}, Secure: {UseSecureChannel}, RetryMaxAttempts: {RetryMaxAttempts}, Interceptors: {InterceptorCount}",
            rawUrl,
            generation,
            useSecureChannel,
            config.RetryMaxAttempts,
            _interceptors.Count);

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(config.PooledConnectionLifetimeMinutes),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(config.PooledConnectionIdleTimeoutSeconds),
            MaxConnectionsPerServer = config.MaxConnectionsPerServer,
            EnableMultipleHttp2Connections = true
        };

        var retryPolicy = new RetryPolicy
        {
            MaxAttempts = config.RetryMaxAttempts,
            InitialBackoff = TimeSpan.FromSeconds(2),
            MaxBackoff = TimeSpan.FromSeconds(10),
            BackoffMultiplier = 1.5,
            RetryableStatusCodes = { StatusCode.Unavailable }
        };

        var channel = GrpcChannel.ForAddress(uri, new GrpcChannelOptions
        {
            HttpHandler = handler,
            Credentials = useSecureChannel
                ? ChannelCredentials.SecureSsl
                : ChannelCredentials.Insecure,
            ServiceConfig = new ServiceConfig
            {
                MethodConfigs =
                {
                    new MethodConfig
                    {
                        Names = { MethodName.Default },
                        RetryPolicy = retryPolicy
                    }
                },
                LoadBalancingConfigs =
                {
                    new RoundRobinConfig()
                }
            },
            ServiceProvider = _serviceProvider
        });

        var invoker = BuildInvoker(channel);

        return new ChannelInstance(channel, rawUrl, invoker, generation);
    }

    private static bool ResolveSecureMode(Uri uri, GrpcClientConfiguration config)
    {
        // safest default: URL decides
        var secureFromUrl = uri.Scheme == Uri.UriSchemeHttps;

        // optional override if you really want to force it from config
        // but reject invalid combinations early
        if (config.UseSecureSslChannel && uri.Scheme == Uri.UriSchemeHttp)
        {
            throw new InvalidOperationException(
                $"gRPC URL '{uri}' uses 'http' but configuration requires a secure SSL channel. " +
                "Use an https URL or disable UseSecureSslChannel.");
        }

        if (!config.UseSecureSslChannel && uri.Scheme == Uri.UriSchemeHttps)
        {
            return true;
        }

        return secureFromUrl;
    }

    private CallInvoker BuildInvoker(GrpcChannel channel)
    {
        CallInvoker invoker = channel.CreateCallInvoker();

        foreach (var interceptor in _interceptors)
        {
            invoker = invoker.Intercept(interceptor);
        }

        return invoker;
    }

    private static Uri ParseAndValidateUri(string rawUrl)
    {
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"Invalid gRPC URL '{rawUrl}'.");
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException(
                $"Unsupported gRPC URL scheme '{uri.Scheme}' in '{rawUrl}'. Only http and https are supported.");
        }

        if (string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new InvalidOperationException($"Invalid gRPC URL '{rawUrl}'. Host is missing.");
        }

        return uri;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            foreach (var pair in _channels)
            {
                try
                {
                    pair.Value.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to dispose gRPC channel for {ChannelUrl}", pair.Key);
                }
            }

            _channels.Clear();
            _disposed = true;
        }
    }
}