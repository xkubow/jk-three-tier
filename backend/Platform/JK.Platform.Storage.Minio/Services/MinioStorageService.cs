using JK.Platform.Core.BlobStorage;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace JK.Platform.Storage.Minio.Services;

[Injectable(ServiceLifetime.Scoped)]
public class MinioStorageService : IBlobStorageService
{
    private readonly IMinioClient _minioClient;

    public MinioStorageService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task UploadAsync(string bucketName, string objectName, Stream data, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(bucketName, cancellationToken);

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
    }

    public async Task<Stream> DownloadAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);

        await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);

            await _minioClient.StatObjectAsync(statObjectArgs, cancellationToken);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }

    public async Task<string> GetLinkAsync(string bucketName, string objectName, int expiryInSeconds = 3600, CancellationToken cancellationToken = default)
    {
        var presignedGetObjectArgs = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expiryInSeconds);

        return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
    }

    private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
        var found = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);
        if (!found)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
        }
    }
}
