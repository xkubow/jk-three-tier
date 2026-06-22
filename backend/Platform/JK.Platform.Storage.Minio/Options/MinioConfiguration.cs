namespace JK.Platform.Storage.Minio.Options;

public class MinioConfiguration
{
    public const string SectionName = "Minio";
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string? Region { get; set; } = "us-east-1";
    public bool Secure { get; set; } = true;
}
