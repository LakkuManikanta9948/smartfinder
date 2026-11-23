using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Domain.Entities;

public class Wishlist
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual required Product Product { get; set; }
    public virtual required int ProductId { get; set; }
}