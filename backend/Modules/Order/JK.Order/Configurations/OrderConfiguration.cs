
using JK.Platform.Core.Configuration;

namespace JK.Order.Configurations;

public sealed class OrderConfiguration : IAppConfiguration
{
    public string SectionName => "Order";

    public int RetryCount { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool EnableAutoProcessing { get; set; }
}