using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDealFinder.Models.Core;

public abstract class ModelBaseEmpty
{
    public short Status { get; set; } = 1;

    [NotMapped]
    public string Error { get; set; } = string.Empty;

    [NotMapped]
    public string Warning { get; set; } = string.Empty;

    [NotMapped]
    public string Success { get; set; } = string.Empty;
}
