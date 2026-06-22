using Microsoft.Extensions.Logging;

namespace JK.Offer.Extensions;

public static partial class OfferServiceLogs
{
    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Information,
        Message = "Updating offer {Id} with status {Status} and total amount {TotalAmount} and expires at {ExpiresAt}.",
        EventName = "OfferService.UpdateAsync")]
    public static partial void LogUpdateOffer(this ILogger logger, Guid id, string status, decimal totalAmount, DateTime expiresAt);
}