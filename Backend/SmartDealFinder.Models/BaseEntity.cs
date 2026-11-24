using SmartDealFinder.Models.Core;

namespace SmartDealFinder.Models;

public abstract class BaseEntity : ModelBase
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}