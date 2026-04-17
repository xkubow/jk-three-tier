namespace JK.Platform.Core.Configuration;

public static class ServerConfigurationKeys
{
    public const string ConnectionString = "ConnectionStrings:DefaultConnection";
    public const string SensitiveDataLogging = "SensitiveDataLogging";
    public const string ConfigurationsReloadEnabled = "Configurations.ReloadEnabled";
    public const string UseSwagger = "UseSwagger";

    public const string Market = "Market";
    public const string MarketArea = "MarketArea";
    public const string ServiceName = "ServiceName";
    public const string ConfigurationsServerUrlKey = "Configurations.Server.Url";
    public const string ConfigurationsReloadPeriodInMilliseconds = "Configurations.ReloadPeriodInMilliseconds";
    public const string ConfigurationsInitialRetryMilliseconds = "Configurations.InitialRetryInMilliseconds";
}