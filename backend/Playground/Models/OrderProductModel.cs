namespace JK.Playground.Models;

public class OrderProductModel
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public int Quantity { get; set; }
}