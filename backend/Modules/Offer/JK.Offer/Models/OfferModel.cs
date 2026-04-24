using JK.Platform.Persistence.EfCore;

namespace JK.Offer.Models;

public class OfferModel : ModelBase<Guid>
{
    public string OfferNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
