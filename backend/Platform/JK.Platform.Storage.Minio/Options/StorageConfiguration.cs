namespace JK.Platform.Storage.Minio.Options;

public class StorageConfiguration
{
    public const string SectionName = "Storage";
    public string Provider { get; set; } = string.Empty;
}
