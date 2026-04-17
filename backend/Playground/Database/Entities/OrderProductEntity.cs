using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JK.Playground.Database.Entities;

[Table("OrderProducts")]
public class OrderProductEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public ProductEntity Product { get; set; }
    public OrderEntity Order { get; set; }
}