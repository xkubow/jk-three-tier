using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JK.Playground.Database.Entities;

[Table("Orders")]
public class OrderEntity
{
    [Key]
    public Guid Id { get; set; }
    public Guid ConsumerId { get; set; }

    public ICollection<OrderProductEntity> Products { get; set; }
}