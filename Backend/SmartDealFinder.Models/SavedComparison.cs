using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class SavedComparison : BaseEntity
{
    [Required, StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [StringLength(80)]
    public string Title { get; set; } = string.Empty;
}
