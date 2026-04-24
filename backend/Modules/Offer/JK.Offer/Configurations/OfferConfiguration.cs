using JK.Platform.Core.Configuration;

namespace JK.Offer.Configurations;

public sealed class OfferConfiguration : IAppConfiguration
{
    public string SectionName => "Offer";

    public int RetryCount { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool EnableAutoProcessing { get; set; }
}
