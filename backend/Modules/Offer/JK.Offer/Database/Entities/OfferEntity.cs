using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JK.Platform.Persistence.EfCore;

namespace JK.Offer.Database.Entities;

[Table("Offer")]
public class OfferEntity: EntityBase<Guid>, ICreatedOnEntity, IUpdatedOnEntity
{
    [Required]
    [MaxLength(100)]
    public string Number { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}
