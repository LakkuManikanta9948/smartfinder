namespace SmartDealFinder.Models.DTO;

public sealed class CatalogProductItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal BestAvailablePrice { get; set; }
    public decimal? VerifiedDiscountPercent { get; set; }
    public bool HasVerifiedPrice { get; set; }
}
