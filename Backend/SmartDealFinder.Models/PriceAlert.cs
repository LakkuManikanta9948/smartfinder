using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDealFinder.Models;

public sealed class PriceAlert : BaseEntity
{
    [Required, StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TargetPrice { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? LastTriggeredAt { get; set; }
}
