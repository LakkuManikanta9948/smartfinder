using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class AdminActivityLog
{
    public long Id { get; set; }

    [Required, StringLength(450)]
    public string ActorUserId { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Subject { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
