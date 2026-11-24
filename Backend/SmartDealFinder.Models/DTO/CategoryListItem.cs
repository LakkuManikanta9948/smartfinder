namespace SmartDealFinder.Models.DTO;

public class CategoryListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public bool Active { get; set; }
}
