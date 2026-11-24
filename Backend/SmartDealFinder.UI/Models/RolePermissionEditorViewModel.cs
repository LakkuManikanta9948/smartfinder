namespace SmartDealFinder.UI.Models;

public sealed class RolePermissionEditorViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public IReadOnlyList<PermissionUserOption> Roles { get; set; } = [];
    public List<PermissionToggle> Permissions { get; set; } = [];
}
