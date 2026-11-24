using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Service;
using SmartDealFinder.UI.Models;

namespace SmartDealFinder.UI.Controllers;

[Authorize(Policy = PermissionCodes.DashboardView)]
[Route("admin")]
public sealed class AdminDashboardController(
    DatabaseContext database,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var roles = database.UserRoles.AsNoTracking();
        var adminCount = await (from membership in roles
                                join role in database.Roles on membership.RoleId equals role.Id
                                where role.Name == "ADMIN"
                                select membership.UserId).Distinct().CountAsync(cancellationToken);
        var model = new AdminDashboardViewModel
        {
            TotalUsers = await userManager.Users.CountAsync(cancellationToken),
            TotalAdmins = adminCount,
            ActiveUsers = await userManager.Users.CountAsync(item => item.IsActive, cancellationToken),
            InactiveUsers = await userManager.Users.CountAsync(item => !item.IsActive, cancellationToken),
            TotalProducts = await database.Products.CountAsync(item => item.Status == 1, cancellationToken),
            TotalCategories = await database.Categories.CountAsync(item => item.Status == 1, cancellationToken),
            TotalComparisons = await database.SavedComparisons.CountAsync(cancellationToken),
            EnabledPlatforms = await database.ShoppingPlatforms.CountAsync(item => item.IsEnabled, cancellationToken),
            TotalPlatforms = await database.ShoppingPlatforms.CountAsync(cancellationToken),
            RegistrationAnalytics = await GetRegistrationAnalyticsAsync(cancellationToken),
            RecentActivities = await GetRecentActivitiesAsync(cancellationToken)
        };
        return View(model);
    }

    private async Task<IReadOnlyList<RegistrationAnalyticsDay>> GetRegistrationAnalyticsAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var start = today.AddDays(-6);
        var dailyCounts = await userManager.Users.AsNoTracking()
            .Where(user => user.CreatedAt >= start && user.CreatedAt < today.AddDays(1))
            .GroupBy(user => user.CreatedAt.Date)
            .Select(group => new { Date = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var counts = dailyCounts.ToDictionary(item => item.Date, item => item.Count);
        return Enumerable.Range(0, 7)
            .Select(offset => start.AddDays(offset))
            .Select(date => new RegistrationAnalyticsDay
            {
                Date = date,
                Registrations = counts.GetValueOrDefault(date)
            })
            .ToList();
    }

    private async Task<IReadOnlyList<AdminActivityItem>> GetRecentActivitiesAsync(CancellationToken cancellationToken)
    {
        var activities = await database.AdminActivityLogs.AsNoTracking()
            .OrderByDescending(activity => activity.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);
        var actorIds = activities.Select(activity => activity.ActorUserId).Distinct().ToList();
        var actors = await userManager.Users.AsNoTracking()
            .Where(user => actorIds.Contains(user.Id))
            .Select(user => new { user.Id, user.DisplayName, user.PhoneNumber })
            .ToDictionaryAsync(user => user.Id, cancellationToken);
        return activities.Select(activity => new AdminActivityItem
        {
            ActorName = actors.TryGetValue(activity.ActorUserId, out var actor)
                ? actor.DisplayName
                : "Former account",
            Action = activity.Action,
            Subject = activity.Subject,
            CreatedAt = activity.CreatedAt
        }).ToList();
    }
}
