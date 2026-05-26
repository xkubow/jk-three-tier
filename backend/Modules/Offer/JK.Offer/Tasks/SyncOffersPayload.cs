namespace JK.Offer.Tasks;

public sealed class SyncOffersPayload
{
    public string ExternalStoreCode { get; set; } = default!;

    public int ChunkSize { get; set; } = 1000;

    public bool FullSync { get; set; } = true;
}
