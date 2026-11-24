using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDealFinder.Models.Core;

public abstract class ModelBase : ModelBaseEmpty
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public long? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
