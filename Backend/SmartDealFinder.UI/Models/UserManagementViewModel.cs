using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.UI.Models;

public sealed class UserManagementViewModel
{
    public string? Search { get; init; }
    public IReadOnlyList<ManagedUserItem> Users { get; init; } = [];
}

public sealed class ManagedUserItem
{
    public string Id { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = "USER";
    public bool IsActive { get; init; }
}

public sealed class CreateManagedUserInput
{
    [Required, RegularExpression("^\\+[1-9]\\d{7,14}$", ErrorMessage = "Enter a phone number in E.164 format, such as +14155552671.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "USER";
}
