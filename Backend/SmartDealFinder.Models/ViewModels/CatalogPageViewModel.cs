using SmartDealFinder.Models.DTO;

namespace SmartDealFinder.Models.ViewModels;

public sealed class CatalogPageViewModel
{
    public IReadOnlyList<CatalogProductItem> Products { get; init; } = [];
    public IReadOnlyList<CategoryListItem> Categories { get; init; } = [];
    public IReadOnlyList<CatalogFilterOption> Brands { get; init; } = [];
    public IReadOnlyList<CatalogFilterOption> Platforms { get; init; } = [];
    public string? Query { get; init; }
    public int? CategoryId { get; init; }
    public int? BrandId { get; init; }
    public int? PlatformId { get; init; }
    public decimal? MinimumPrice { get; init; }
    public decimal? MaximumPrice { get; init; }
    public decimal? MinimumRating { get; init; }
    public bool? InStock { get; init; }
    public string Sort { get; init; } = "popular";
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public int TotalItems { get; init; }
}
