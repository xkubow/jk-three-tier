namespace JK.Offer.Contracts;

public class CreateOfferRequest
{
    public string Number { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime ExpiresAt { get; set; }
}
