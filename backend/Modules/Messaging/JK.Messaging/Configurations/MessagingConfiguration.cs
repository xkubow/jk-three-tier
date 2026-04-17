using JK.Platform.Core.Configuration;

namespace JK.Messaging.Configurations;

public sealed class MessagingConfiguration : IAppConfiguration
{
    public string SectionName => "Messaging";

    public int RetryCount { get; set; }

    public int TimeoutSeconds { get; set; }

    public bool EnableOrleansEcho { get; set; } = true;
}
