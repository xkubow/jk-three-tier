using JK.Configuration.Proto;
using JK.Platform.Grpc.Client.Factory;
using Microsoft.Extensions.Configuration;

namespace JK.Configuration.Provider;

public sealed class ConfigurationServerSource : IConfigurationSource
{
    public IGrpcClientFactory<ConfigurationGrpc.ConfigurationGrpcClient> GrpcClientFactory { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new ConfigurationServerProvider(builder, this);

}