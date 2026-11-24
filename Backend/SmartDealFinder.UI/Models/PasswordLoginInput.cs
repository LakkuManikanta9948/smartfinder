using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.UI.Models;

public sealed class PasswordLoginInput
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [StringLength(2048)]
    public string? ReturnUrl { get; set; }
}
