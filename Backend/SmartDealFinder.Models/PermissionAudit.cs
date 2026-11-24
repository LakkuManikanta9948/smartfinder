using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class PermissionAudit
{
    public long Id { get; set; }

    [Required, StringLength(450)]
    public string ActorUserId { get; set; } = string.Empty;

    [Required, StringLength(450)]
    public string SubjectUserId { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string PermissionCode { get; set; } = string.Empty;

    public bool IsGranted { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
