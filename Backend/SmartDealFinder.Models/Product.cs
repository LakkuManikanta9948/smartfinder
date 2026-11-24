using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartDealFinder.Models.Core;

namespace SmartDealFinder.Models;

[Table("products")]
public class Product : ModelBase
{


    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(40)]
    public string? MerchantSku { get; set; }

    [StringLength(80)]
    public string? ModelNumber { get; set; }

    [StringLength(14)]
    public string? Gtin { get; set; }

    [StringLength(120)]
    public string? Variant { get; set; }

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

    [Column(TypeName = "decimal(3,2)")]
    public decimal? Rating { get; set; }

    public int? ReviewCount { get; set; }

    public int? BrandId { get; set;}
    public Brand? Brand { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }

    public int CategoryId { get; set; }

    [ForeignKey("MerchantId")]
    public virtual Merchant? Merchant { get; set; }

    public int MerchantId { get; set; }

    public ICollection<PlatformListing> PlatformListings { get; set; } = new List<PlatformListing>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
}