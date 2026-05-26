namespace JK.Offer.Tasks.External;

public sealed class ExternalOfferPage
{
    public IReadOnlyList<ExternalOfferDto> Items { get; init; } = [];

    public string? NextCursor { get; init; }
}
