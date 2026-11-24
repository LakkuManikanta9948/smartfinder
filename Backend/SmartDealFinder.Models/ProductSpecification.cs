using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class ProductSpecification : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Value { get; set; } = string.Empty;

    public bool IsVerifiedSource { get; set; }
}
