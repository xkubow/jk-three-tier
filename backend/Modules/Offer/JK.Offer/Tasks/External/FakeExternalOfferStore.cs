using Microsoft.Extensions.Options;

namespace JK.Offer.Tasks.External;

public sealed class FakeExternalOfferStore : IExternalOfferStore
{
    private readonly FakeExternalOfferStoreOptions _options;

    public FakeExternalOfferStore(IOptions<FakeExternalOfferStoreOptions> options)
    {
        _options = options.Value;
    }

    public Task<long> GetTotalCountAsync(string externalStoreCode, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetCountForStore(externalStoreCode));
    }

    public Task<ExternalOfferPage> GetPageAsync(
        string externalStoreCode,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var total = GetCountForStore(externalStoreCode);
        var offset = string.IsNullOrWhiteSpace(cursor) ? 0L : long.Parse(cursor);
        var take = Math.Min(limit, total - offset);

        if (take <= 0)
        {
            return Task.FromResult(new ExternalOfferPage
            {
                Items = [],
                NextCursor = null
            });
        }

        var items = new List<ExternalOfferDto>((int)take);

        for (var i = offset; i < offset + take; i++)
        {
            items.Add(new ExternalOfferDto
            {
                Number = $"{externalStoreCode}-{i:D8}",
                TotalAmount = (i % 1000) + 0.99m,
                Status = "Active",
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });
        }

        var nextOffset = offset + take;
        var nextCursor = nextOffset < total ? nextOffset.ToString() : null;

        return Task.FromResult(new ExternalOfferPage
        {
            Items = items,
            NextCursor = nextCursor
        });
    }

    private long GetCountForStore(string externalStoreCode)
    {
        if (_options.StoreOfferCounts.TryGetValue(externalStoreCode, out var count))
            return count;

        return _options.DefaultOfferCount;
    }
}
