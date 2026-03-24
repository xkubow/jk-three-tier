using JK.Platform.Grpc.Server.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Grpc.Server.Extensions;

public static class GrpcPlatformServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcPlatform(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        }).AddJsonTranscoding();
        services.AddGrpcSwagger();

        return services;
    }
}