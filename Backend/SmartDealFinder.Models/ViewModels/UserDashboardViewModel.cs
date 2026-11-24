using SmartDealFinder.Models.DTO;

namespace SmartDealFinder.Models.ViewModels;

public sealed class UserDashboardViewModel
{
    public IReadOnlyList<CatalogProductItem> Wishlist { get; init; } = [];
    public IReadOnlyList<UserPriceAlertItem> PriceAlerts { get; init; } = [];
    public IReadOnlyList<UserSavedComparisonItem> SavedComparisons { get; init; } = [];
    public IReadOnlyList<CatalogProductItem> RecentlyViewed { get; init; } = [];
    public IReadOnlyList<UserNotificationItem> Notifications { get; init; } = [];
}

public sealed class UserPriceAlertItem
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public decimal TargetPrice { get; init; }
    public bool IsActive { get; init; }
}

public sealed class UserNotificationItem
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
