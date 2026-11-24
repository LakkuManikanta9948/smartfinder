using SmartDealFinder.Models;

namespace SmartDealFinder.Service;

public sealed record PlatformListingSnapshot(
    string PlatformName,
    string PlatformWebsite,
    string Title,
    decimal CurrentPrice,
    decimal? OriginalPrice,
    string Currency,
    bool IsAvailable,
    bool IsVerifiedLiveData,
    string DataSource,
    DateTime LastUpdatedAt,
    string? ProductUrl = null,
    string? ImageUrl = null);

public interface IPlatformProductProvider
{
    Task<IReadOnlyList<PlatformListingSnapshot>> GetListingsAsync(Product product, CancellationToken cancellationToken = default);
}
