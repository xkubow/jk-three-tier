using JK.Platform.Core.BlobStorage;
using JK.Platform.Storage.Minio.Health;
using JK.Platform.Storage.Minio.Options;
using JK.Platform.Storage.Minio.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;

namespace JK.Platform.Storage.Minio.Extensions;

public static class MinioServiceCollectionExtensions
{
    public static IServiceCollection AddMinioStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageConfiguration>(configuration.GetSection(StorageConfiguration.SectionName));

        var storageConfig = configuration.GetSection(StorageConfiguration.SectionName).Get<StorageConfiguration>();
        if (storageConfig != null && !string.Equals(storageConfig.Provider, "Minio", StringComparison.OrdinalIgnoreCase))
        {
            return services;
        }

        services.Configure<MinioConfiguration>(configuration.GetSection(MinioConfiguration.SectionName));

        services.AddSingleton<IMinioClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<MinioConfiguration>>().Value;
            var client = new MinioClient()
                .WithEndpoint(config.Endpoint)
                .WithCredentials(config.AccessKey, config.SecretKey)
                .WithSSL(config.Secure);

            if (!string.IsNullOrEmpty(config.Region))
            {
                client.WithRegion(config.Region);
            }

            return client.Build();
        });

        services.AddHealthChecks()
            .AddCheck<MinioHealthCheck>("Minio");

        return services;
    }
}
