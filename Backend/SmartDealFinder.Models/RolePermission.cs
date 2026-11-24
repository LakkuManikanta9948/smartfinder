using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class RolePermission
{
    public int Id { get; set; }

    [Required, StringLength(450)]
    public string RoleId { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string PermissionCode { get; set; } = string.Empty;

    public bool IsGranted { get; set; }
}
