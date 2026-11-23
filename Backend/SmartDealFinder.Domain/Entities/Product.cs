using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDealFinder.Domain.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(40)]
    public string? MerchantSku { get; set; }

    [StringLength(200)]
    public string? ImageUrl { get; set; }

    [StringLength(40)]
    public string? Currency { get; set; } = "INR";

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OriginalPrice { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DiscountPercent { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LowestPrice { get; set; }

    public DateTime? LastPriceUpdate { get; set; }

    public bool Active { get; set; } = true;

    public bool Featured { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual required Category Category { get; set; }

    public required int CategoryId { get; set; }

    public virtual required Merchant Merchant { get; set; }

    public required int MerchantId { get; set; }
}