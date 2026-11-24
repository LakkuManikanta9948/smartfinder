namespace SmartDealFinder.Models.ViewModels;

public sealed class PriceHistoryPoint
{
    public string PlatformName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public DateTime CapturedAt { get; init; }
}
