using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartDealFinder.Models.Core;

namespace SmartDealFinder.Models;

[Table("categories")]
public class Category : ModelBase
{


    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(160)]
    public string? Slug { get; set; }

    [StringLength(20)]
    public string? IconClass { get; set; }

    public int? ParentId { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Brand> Brands { get; set; } = new List<Brand>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}