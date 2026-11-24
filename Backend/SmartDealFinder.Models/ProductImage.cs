using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(160)]
    public string? AltText { get; set; }

    public int SortOrder { get; set; }
    public bool IsVerifiedSource { get; set; }
}
