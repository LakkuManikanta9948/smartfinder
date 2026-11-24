using SmartDealFinder.Models;
using SmartDealFinder.Service;

namespace SmartDealFinder.UI.Services;

public sealed class DevelopmentSampleProductProvider : IPlatformProductProvider
{
    public Task<IReadOnlyList<PlatformListingSnapshot>> GetListingsAsync(Product product, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        IReadOnlyList<PlatformListingSnapshot> samples =
        [
            new("Amazon India", "https://www.amazon.in", product.Title, decimal.Round(product.CurrentPrice * 0.96m, 2), null, product.Currency ?? "INR", true, false, "Development sample — not a live offer", now),
            new("Flipkart", "https://www.flipkart.com", product.Title, decimal.Round(product.CurrentPrice * 0.99m, 2), null, product.Currency ?? "INR", true, false, "Development sample — not a live offer", now)
        ];
        return Task.FromResult(samples);
    }
}
