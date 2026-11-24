using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDealFinder.Models;

public sealed class PlatformListing : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int ShoppingPlatformId { get; set; }
    public ShoppingPlatform ShoppingPlatform { get; set; } = null!;

    [Required, StringLength(240)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1500)]
    public string ProductUrl { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OriginalPrice { get; set; }

    [StringLength(8)]
    public string Currency { get; set; } = "INR";

    public bool IsAvailable { get; set; }
    public bool IsVerifiedLiveData { get; set; }

    [Required, StringLength(40)]
    public string DataSource { get; set; } = "Development sample";

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
