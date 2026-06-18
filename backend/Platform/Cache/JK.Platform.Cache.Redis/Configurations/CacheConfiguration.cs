namespace JK.Platform.Cache.Redis.Configurations;

public class CacheConfiguration
{
    public string SectionName => "CacheService";
    public int DefaultExpirationSecond { get; set; }
}