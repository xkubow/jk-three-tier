using Microsoft.Extensions.Diagnostics.HealthChecks;
using Minio;

namespace JK.Platform.Storage.Minio.Health;

public class MinioHealthCheck : IHealthCheck
{
    private readonly IMinioClient _minioClient;

    public MinioHealthCheck(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _minioClient.ListBucketsAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Minio health check failed", ex);
        }
    }
}
