using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;

namespace SmartDealFinder.UI.Services;

public sealed class VerifiedPriceHistoryCollector(IServiceScopeFactory scopeFactory, ILogger<VerifiedPriceHistoryCollector> logger) : BackgroundService
{
    private static readonly TimeSpan CaptureInterval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CaptureInterval);
        while (!stoppingToken.IsCancellationRequested)
        {
            await CapturePricesAsync(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task CapturePricesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var cutoff = DateTime.UtcNow.Subtract(CaptureInterval);
        var listings = await database.PlatformListings
            .Where(listing => listing.Status == 1 && listing.IsAvailable && listing.IsVerifiedLiveData
                && listing.ShoppingPlatform.IsEnabled)
            .Select(listing => new
            {
                Listing = listing,
                LastCapturedAt = database.ProductPriceHistory
                    .Where(history => history.PlatformListingId == listing.Id && history.IsVerifiedLiveData)
                    .Select(history => (DateTime?)history.CapturedAt)
                    .Max()
            })
            .Where(item => item.LastCapturedAt == null || item.LastCapturedAt < cutoff)
            .ToListAsync(cancellationToken);

        foreach (var item in listings)
        {
            database.ProductPriceHistory.Add(new ProductPriceHistory
            {
                PlatformListingId = item.Listing.Id,
                Price = item.Listing.CurrentPrice,
                CapturedAt = DateTime.UtcNow,
                IsVerifiedLiveData = true
            });
        }

        if (listings.Count == 0)
            return;

        await database.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Captured {PriceCount} verified retailer prices for price history.", listings.Count);
    }
}
