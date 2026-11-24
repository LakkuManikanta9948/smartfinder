namespace SmartDealFinder.Models.DTO;

public class ProductListItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool Active { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
}
