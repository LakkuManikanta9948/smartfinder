using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class UserPermission
{
    public int Id { get; set; }

    [Required, StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string PermissionCode { get; set; } = string.Empty;

    public bool IsGranted { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
}
