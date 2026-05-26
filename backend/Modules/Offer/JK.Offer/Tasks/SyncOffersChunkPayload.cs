namespace JK.Offer.Tasks;

public sealed class SyncOffersChunkPayload
{
    public string ExternalStoreCode { get; set; } = default!;

    public string? Cursor { get; set; }

    public int Limit { get; set; }
}
