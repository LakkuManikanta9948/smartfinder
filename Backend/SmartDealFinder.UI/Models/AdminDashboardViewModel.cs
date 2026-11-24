namespace SmartDealFinder.UI.Models;

public sealed class AdminDashboardViewModel
{
    public int TotalUsers { get; init; }
    public int TotalAdmins { get; init; }
    public int ActiveUsers { get; init; }
    public int InactiveUsers { get; init; }
    public int TotalProducts { get; init; }
    public int TotalCategories { get; init; }
    public int TotalComparisons { get; init; }
    public int EnabledPlatforms { get; init; }
    public int TotalPlatforms { get; init; }
    public IReadOnlyList<RegistrationAnalyticsDay> RegistrationAnalytics { get; init; } = [];
    public IReadOnlyList<AdminActivityItem> RecentActivities { get; init; } = [];
}

public sealed class RegistrationAnalyticsDay
{
    public DateTime Date { get; init; }
    public int Registrations { get; init; }
}

public sealed class AdminActivityItem
{
    public string ActorName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
