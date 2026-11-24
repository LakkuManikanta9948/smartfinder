using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class ShoppingPlatform : BaseEntity
{
    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string WebsiteUrl { get; set; } = string.Empty;

    [StringLength(300)]
    public string? LogoUrl { get; set; }

    public bool IsEnabled { get; set; }
    public bool IsDevelopmentProvider { get; set; }
    public DateTime? LastSuccessfulSyncAt { get; set; }

    public ICollection<PlatformListing> Listings { get; set; } = new List<PlatformListing>();
}
