using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Models.DTO;
using SmartDealFinder.Models.ViewModels;

namespace SmartDealFinder.Service;

public interface IUserProductService
{
    Task<UserDashboardViewModel> GetDashboardAsync(string userId, CancellationToken cancellationToken = default);
    Task AddToWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default);
    Task RemoveFromWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default);
    Task<int> AddPriceAlertAsync(string userId, int productId, decimal targetPrice, CancellationToken cancellationToken = default);
    Task RemovePriceAlertAsync(string userId, int alertId, CancellationToken cancellationToken = default);
    Task SaveComparisonAsync(string userId, int productId, CancellationToken cancellationToken = default);
    Task RecordProductViewAsync(string userId, int productId, CancellationToken cancellationToken = default);
}

public sealed class UserProductService(DatabaseContext database) : IUserProductService
{
    public async Task<UserDashboardViewModel> GetDashboardAsync(string userId, CancellationToken cancellationToken = default)
    {
        var savedProducts = await database.Wishlists.AsNoTracking()
            .Where(item => item.UserId == userId && item.Product != null && item.Product.Status == 1)
            .Include(item => item.Product).ThenInclude(item => item!.Brand)
            .Include(item => item.Product).ThenInclude(item => item!.Category)
            .Include(item => item.Product).ThenInclude(item => item!.PlatformListings)
            .Select(item => item.Product!)
            .ToListAsync(cancellationToken);

        var alerts = await database.PriceAlerts.AsNoTracking()
            .Where(item => item.UserId == userId)
            .Include(item => item.Product)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new UserPriceAlertItem
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductTitle = item.Product.Title,
                TargetPrice = item.TargetPrice,
                IsActive = item.IsActive
            }).ToListAsync(cancellationToken);

        var savedComparisons = await database.SavedComparisons.AsNoTracking()
            .Where(item => item.UserId == userId)
            .Include(item => item.Product)
            .OrderByDescending(item => item.CreatedAt)
            .Take(20)
            .Select(item => new UserSavedComparisonItem
            {
                ProductId = item.ProductId,
                ProductTitle = item.Product.Title,
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var recentIds = (await database.UserActivityLogs.AsNoTracking()
                .Where(item => item.UserId == userId && item.ActivityType == "ProductViewed" && item.ProductId.HasValue)
                .OrderByDescending(item => item.CreatedAt)
                .Select(item => item.ProductId!.Value)
                .Take(30)
                .ToListAsync(cancellationToken))
            .Distinct()
            .Take(8)
            .ToList();
        var recentProducts = await database.Products.AsNoTracking()
            .Where(item => recentIds.Contains(item.Id) && item.Status == 1 && item.Active)
            .Include(item => item.Brand)
            .Include(item => item.Category)
            .Include(item => item.PlatformListings)
            .ToListAsync(cancellationToken);
        var recentOrdered = recentIds.Select(id => recentProducts.FirstOrDefault(item => item.Id == id))
            .Where(item => item != null)
            .Select(item => ToCatalogItem(item!))
            .ToList();

        var notifications = await database.Notifications.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .Take(10)
            .Select(item => new UserNotificationItem { Title = item.Title, Message = item.Message, CreatedAt = item.CreatedAt })
            .ToListAsync(cancellationToken);

        return new UserDashboardViewModel
        {
            Wishlist = savedProducts.Select(ToCatalogItem).ToList(),
            PriceAlerts = alerts,
            SavedComparisons = savedComparisons,
            RecentlyViewed = recentOrdered,
            Notifications = notifications
        };
    }

    public async Task AddToWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default)
    {
        var exists = await database.Wishlists.AnyAsync(item => item.UserId == userId && item.ProductId == productId, cancellationToken);
        if (exists)
            return;

        var productExists = await database.Products.AnyAsync(item => item.Id == productId && item.Status == 1 && item.Active, cancellationToken);
        if (!productExists)
            throw new KeyNotFoundException("The selected product is not available.");

        database.Wishlists.Add(new Wishlist { UserId = userId, ProductId = productId });
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default)
    {
        var saved = await database.Wishlists.SingleOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId, cancellationToken);
        if (saved == null)
            return;

        database.Wishlists.Remove(saved);
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AddPriceAlertAsync(string userId, int productId, decimal targetPrice, CancellationToken cancellationToken = default)
    {
        if (targetPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetPrice), "Target price must be greater than zero.");

        var productExists = await database.Products.AnyAsync(item => item.Id == productId && item.Status == 1 && item.Active, cancellationToken);
        if (!productExists)
            throw new KeyNotFoundException("The selected product is not available.");

        var alert = await database.PriceAlerts.FirstOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId && item.IsActive, cancellationToken);
        if (alert == null)
        {
            alert = new PriceAlert { UserId = userId, ProductId = productId, TargetPrice = targetPrice };
            database.PriceAlerts.Add(alert);
        }
        else
        {
            alert.TargetPrice = targetPrice;
            alert.LastTriggeredAt = null;
        }

        await database.SaveChangesAsync(cancellationToken);
        return alert.Id;
    }

    public async Task RemovePriceAlertAsync(string userId, int alertId, CancellationToken cancellationToken = default)
    {
        var alert = await database.PriceAlerts.SingleOrDefaultAsync(item => item.UserId == userId && item.Id == alertId, cancellationToken);
        if (alert == null)
            return;

        alert.IsActive = false;
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveComparisonAsync(string userId, int productId, CancellationToken cancellationToken = default)
    {
        var productExists = await database.Products.AnyAsync(item => item.Id == productId && item.Status == 1 && item.Active, cancellationToken);
        if (!productExists)
            throw new KeyNotFoundException("The selected product is not available.");

        var alreadySaved = await database.SavedComparisons.AnyAsync(item => item.UserId == userId && item.ProductId == productId, cancellationToken);
        if (alreadySaved)
            return;

        database.SavedComparisons.Add(new SavedComparison { UserId = userId, ProductId = productId, Title = "Product comparison" });
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordProductViewAsync(string userId, int productId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        database.UserActivityLogs.Add(new UserActivityLog { UserId = userId, ProductId = productId, ActivityType = "ProductViewed" });
        await database.SaveChangesAsync(cancellationToken);
    }

    private static CatalogProductItem ToCatalogItem(Product product)
    {
        var verifiedAvailableListings = product.PlatformListings
            .Where(item => item.IsVerifiedLiveData && item.IsAvailable)
            .ToList();
        var discount = verifiedAvailableListings
            .Where(item => item.OriginalPrice.HasValue && item.OriginalPrice.Value > item.CurrentPrice)
            .Select(item => (decimal?)((item.OriginalPrice!.Value - item.CurrentPrice) / item.OriginalPrice.Value * 100))
            .Max();

        return new CatalogProductItem
        {
            Id = product.Id,
            Title = product.Title,
            BrandName = product.Brand?.Name,
            CategoryName = product.Category?.Name,
            ImageUrl = product.ImageUrl,
            StartingPrice = product.CurrentPrice,
            BestAvailablePrice = verifiedAvailableListings.Select(item => (decimal?)item.CurrentPrice).Min() ?? product.CurrentPrice,
            VerifiedDiscountPercent = discount,
            HasVerifiedPrice = verifiedAvailableListings.Count > 0
        };
    }
}
