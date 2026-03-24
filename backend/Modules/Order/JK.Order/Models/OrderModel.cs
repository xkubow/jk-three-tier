using JK.Platform.Persistence.EfCore;

namespace JK.Order.Models;

public class OrderModel : ModelBase<Guid>
{
    public string Number { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
