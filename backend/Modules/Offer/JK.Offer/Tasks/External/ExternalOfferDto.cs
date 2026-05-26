namespace JK.Offer.Tasks.External;

public sealed class ExternalOfferDto
{
    public string Number { get; set; } = default!;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "Active";

    public DateTime ExpiresAt { get; set; }
}
