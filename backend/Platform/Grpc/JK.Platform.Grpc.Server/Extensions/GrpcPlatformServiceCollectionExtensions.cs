using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Grpc.Server.Extensions;

public static class GrpcPlatformServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcPlatform(this IServiceCollection services)
    {
        services.AddGrpc().AddJsonTranscoding();
        services.AddGrpcSwagger();

        return services;
    }
}