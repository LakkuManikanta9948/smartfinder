namespace SmartDealFinder.Models.ViewModels;

public sealed class ComparisonPageViewModel
{
    public int ProductId { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public IReadOnlyList<ComparisonOfferViewModel> Offers { get; init; } = [];
    public decimal? LowestVerifiedPrice => Offers.Where(x => x.IsAvailable && x.IsVerifiedLiveData).Select(x => (decimal?)x.CurrentPrice).Min();
    public decimal? HighestVerifiedPrice => Offers.Where(x => x.IsAvailable && x.IsVerifiedLiveData).Select(x => (decimal?)x.CurrentPrice).Max();
    public decimal? PotentialSavings => LowestVerifiedPrice.HasValue && HighestVerifiedPrice.HasValue
        ? HighestVerifiedPrice.Value - LowestVerifiedPrice.Value
        : null;
    public decimal? SavingsPercentage => LowestVerifiedPrice.HasValue && HighestVerifiedPrice is > 0
        ? PotentialSavings / HighestVerifiedPrice.Value * 100
        : null;
    public bool HasSampleOffers => Offers.Any(x => !x.IsVerifiedLiveData);
}

public sealed class ComparisonOfferViewModel
{
    public string PlatformName { get; init; } = string.Empty;
    public string? PlatformLogoUrl { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal? OriginalPrice { get; init; }
    public string Currency { get; init; } = "INR";
    public bool IsAvailable { get; init; }
    public bool IsVerifiedLiveData { get; init; }
    public string DataSource { get; init; } = string.Empty;
    public DateTime LastUpdatedAt { get; init; }
    public string? SafeProductUrl { get; init; }
}
