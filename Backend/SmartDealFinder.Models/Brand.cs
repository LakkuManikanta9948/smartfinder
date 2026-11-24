using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartDealFinder.Models.Core;

namespace SmartDealFinder.Models;

[Table("brands")]
public class Brand : ModelBase
{


    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? LogoUrl { get; set; }

    [StringLength(40)]
    public string? Country { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("Category")]
    public int? CategoryId { get; set; }

    public Category? Category { get; set; }
}