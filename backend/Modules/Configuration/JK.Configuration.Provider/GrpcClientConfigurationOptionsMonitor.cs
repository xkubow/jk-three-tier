using JK.Platform.Grpc.Client;
using Microsoft.Extensions.Options;

namespace JK.Configuration.Provider;

public sealed class GrpcClientConfigurationOptionsMonitor : IOptionsMonitor<GrpcClientConfiguration>
{
    public GrpcClientConfigurationOptionsMonitor(GrpcClientConfiguration currentValue)
    {
        CurrentValue = currentValue;
    }

    public GrpcClientConfiguration CurrentValue { get; }

    public GrpcClientConfiguration Get(string? name)
        => CurrentValue;

    public IDisposable OnChange(Action<GrpcClientConfiguration, string?> listener)
        => EmptyDisposable.Instance;

    private sealed class EmptyDisposable : IDisposable
    {
        public static readonly EmptyDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}