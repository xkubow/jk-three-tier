using JK.Playground.Database.Entities;

namespace JK.Playground.Models;

public class OrderModel
{
    public Guid Id { get; set; }
    public Guid ConsumerId { get; set; }

    public IEnumerable<OrderProductModel> Products { get; set; } = null!;
}