using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Domain.Entities;

public class Merchant
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Website { get; set; }

    [StringLength(200)]
    public string? LogoUrl { get; set; }

    [StringLength(40)]
    public string? Country { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual required Category Category { get; set; }

    public required int CategoryId { get; set; }
}