using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JK.Playground.Database.Entities;

[Table("Configuration")]
public class ConfigurationEntity
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Key { get; set; } = string.Empty;
    [Required]
    public string Value { get; set; } = string.Empty;
}