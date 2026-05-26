namespace JK.Offer.Tasks.External;

public sealed class FakeExternalOfferStoreOptions
{
    public const string SectionName = "FakeExternalOfferStore";

    public long DefaultOfferCount { get; set; } = 10_000;

    public Dictionary<string, long> StoreOfferCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
