namespace JK.Order.Contracts;

public class OrderDto
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

