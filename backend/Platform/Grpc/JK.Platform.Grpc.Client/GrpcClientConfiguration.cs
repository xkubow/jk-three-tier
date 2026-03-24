using JK.Platform.Core.Configuration;

namespace JK.Platform.Grpc.Client;

public sealed class GrpcClientConfiguration
{
    public bool UseSecureSslChannel { get; set; } = false;

    public int RetryMaxAttempts { get; set; } = 3;

    public int PooledConnectionLifetimeMinutes { get; set; } = 5;

    public int PooledConnectionIdleTimeoutSeconds { get; set; } = 60;

    public int MaxConnectionsPerServer { get; set; } = 10;
}