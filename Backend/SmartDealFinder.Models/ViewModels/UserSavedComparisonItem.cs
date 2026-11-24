namespace SmartDealFinder.Models.ViewModels;

public sealed class UserSavedComparisonItem
{
    public int ProductId { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
