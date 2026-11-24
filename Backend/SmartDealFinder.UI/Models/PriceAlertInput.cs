using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.UI.Models;

public sealed class PriceAlertInput
{
    public int ProductId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000000", ErrorMessage = "Target price must be greater than zero.")]
    public decimal TargetPrice { get; set; }
}
