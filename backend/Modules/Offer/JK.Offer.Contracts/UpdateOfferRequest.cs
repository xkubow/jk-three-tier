namespace JK.Offer.Contracts;

public class UpdateOfferRequest
{
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime ExpiresAt { get; set; }
}
