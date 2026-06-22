namespace JK.Platform.Cache.Redis.Configurations;

public class CacheConfiguration
{
    public string Provider { get; set; } = "Redis";
    public string SectionName { get; set; } = "Redis";
    public int DefaultExpirationSecond { get; set; } = 60;
}