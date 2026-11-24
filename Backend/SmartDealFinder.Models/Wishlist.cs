using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartDealFinder.Models.Core;

namespace SmartDealFinder.Models;

[Table("wishlists")]
public class Wishlist : ModelBase
{


    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
    public int ProductId { get; set; }
}