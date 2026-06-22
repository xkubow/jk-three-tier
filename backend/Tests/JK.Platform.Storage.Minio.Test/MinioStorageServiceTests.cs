using System.Reflection;
using System.Text;
using FluentAssertions;
using JK.Platform.Core.BlobStorage;
using JK.Platform.Core.DependencyInjection;
using JK.Platform.Storage.Minio.Extensions;
using JK.Platform.Storage.Minio.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace JK.Platform.Storage.Minio.Test;

public class MinioStorageServiceTests
{
    private readonly IServiceProvider _serviceProvider;

    public MinioStorageServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Storage:Provider", "Minio" },
                { "Minio:Endpoint", "localhost:19000" },
                { "Minio:AccessKey", "minioadmin" },
                { "Minio:SecretKey", "minioadmin" },
                { "Minio:Secure", "false" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMinioStorage(configuration);
        services.RegisterInjectableServices(typeof(MinioStorageService).Assembly);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task UploadAndDownload_ShouldWork()
    {
        // Arrange
        var storageService = _serviceProvider.GetRequiredService<IBlobStorageService>();
        var bucketName = "test-bucket";
        var objectName = $"test-file-{Guid.NewGuid()}.txt";
        var content = "Hello Minio!";
        var data = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var contentType = "text/plain";

        // Act
        await storageService.UploadAsync(bucketName, objectName, data, contentType);
        
        var downloadedStream = await storageService.DownloadAsync(bucketName, objectName);
        using var reader = new StreamReader(downloadedStream);
        var downloadedContent = await reader.ReadToEndAsync();

        // Assert
        downloadedContent.Should().Be(content);

        // Cleanup
        await storageService.DeleteAsync(bucketName, objectName);
        var exists = await storageService.ExistsAsync(bucketName, objectName);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetLink_ShouldReturnValidUrl()
    {
        // Arrange
        var storageService = _serviceProvider.GetRequiredService<IBlobStorageService>();
        var bucketName = "test-bucket";
        var objectName = $"test-link-{Guid.NewGuid()}.txt";
        var content = "Link test";
        var data = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        await storageService.UploadAsync(bucketName, objectName, data, "text/plain");

        // Act
        var link = await storageService.GetLinkAsync(bucketName, objectName);

        // Assert
        link.Should().NotBeNullOrEmpty();
        link.Should().Contain(bucketName);
        link.Should().Contain(objectName);
        link.Should().Contain("localhost:19000");

        // Cleanup
        await storageService.DeleteAsync(bucketName, objectName);
    }

    [Fact]
    public async Task HealthCheck_ShouldBeHealthy()
    {
        // Arrange
        var healthCheckService = _serviceProvider.GetRequiredService<HealthCheckService>();
        
        // Act
        var result = await healthCheckService.CheckHealthAsync();

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Entries.Should().ContainKey("Minio");
        result.Entries["Minio"].Status.Should().Be(HealthStatus.Healthy);
    }
}
