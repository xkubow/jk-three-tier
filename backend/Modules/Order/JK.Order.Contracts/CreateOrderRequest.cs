namespace JK.Order.Contracts;

public class CreateOrderRequest
{
    public string Number { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
}

