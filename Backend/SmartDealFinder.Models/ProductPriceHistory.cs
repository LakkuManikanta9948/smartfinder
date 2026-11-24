using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDealFinder.Models;

public sealed class ProductPriceHistory : BaseEntity
{
    public int PlatformListingId { get; set; }
    public PlatformListing PlatformListing { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public bool IsVerifiedLiveData { get; set; }
}
