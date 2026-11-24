using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.UI.Models;

public sealed class PermissionEditorViewModel
{
    [Required]
    public string TargetUserId { get; set; } = string.Empty;

    public string TargetLabel { get; set; } = string.Empty;
    public IReadOnlyList<PermissionUserOption> Users { get; set; } = [];
    public List<PermissionToggle> Permissions { get; set; } = [];
}

public sealed class PermissionUserOption
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}

public sealed class PermissionToggle
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}
