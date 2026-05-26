namespace JK.Offer.Tasks.External;

public interface IExternalOfferStore
{
    Task<long> GetTotalCountAsync(string externalStoreCode, CancellationToken cancellationToken = default);

    Task<ExternalOfferPage> GetPageAsync(
        string externalStoreCode,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default);
}
