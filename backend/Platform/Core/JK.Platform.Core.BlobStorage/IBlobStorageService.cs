namespace JK.Platform.Core.BlobStorage;

public interface IBlobStorageService
{
    Task UploadAsync(string bucketName, string objectName, Stream data, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    Task<string> GetLinkAsync(string bucketName, string objectName, int expiryInSeconds = 3600, CancellationToken cancellationToken = default);
}
