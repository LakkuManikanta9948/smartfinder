using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;

namespace SmartDealFinder.UI.Services;

public sealed class PriceAlertMonitor(IServiceScopeFactory scopeFactory, ILogger<PriceAlertMonitor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        do
        {
            await CheckAlertsAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckAlertsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var alerts = await database.PriceAlerts
            .Where(item => item.IsActive)
            .Include(item => item.Product)
            .ToListAsync(cancellationToken);
        var notificationCount = 0;

        foreach (var alert in alerts)
        {
            var lowestVerifiedPrice = await database.PlatformListings
                .Where(item => item.ProductId == alert.ProductId && item.IsAvailable && item.IsVerifiedLiveData && item.Status == 1)
                .Select(item => (decimal?)item.CurrentPrice)
                .MinAsync(cancellationToken);
            if (!lowestVerifiedPrice.HasValue || lowestVerifiedPrice.Value > alert.TargetPrice)
                continue;

            database.Notifications.Add(new AppNotification
            {
                UserId = alert.UserId,
                Title = "Your price target was reached",
                Message = $"{alert.Product.Title} now has a verified offer at {lowestVerifiedPrice.Value:N2}, at or below your target of {alert.TargetPrice:N2}."
            });
            notificationCount++;
            alert.IsActive = false;
            alert.LastTriggeredAt = DateTime.UtcNow;
        }

        await database.SaveChangesAsync(cancellationToken);
        if (notificationCount > 0)
            logger.LogInformation("Price alert monitor created {NotificationCount} verified-price notifications.", notificationCount);
    }
}
