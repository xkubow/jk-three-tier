using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Grpc.Server.Extensions;

public static class GrpcPlatformServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcPlatform(this IServiceCollection services)
    {
        services.AddGrpc().AddJsonTranscoding();
        services.AddGrpcSwagger();
        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddGrpcSwagger(this IServiceCollection services, string title, string version = "v1")
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(version, new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = title,
                Version = version
            });
        });

        return services;
    }
}