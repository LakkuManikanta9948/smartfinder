using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.UI.Models;

public sealed class RequestOtpInput
{
    [Required, StringLength(32)]
    [RegularExpression(@"^\+[1-9]\d{7,14}$", ErrorMessage = "Use international phone format, for example +14155552671.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(2048)]
    public string? ReturnUrl { get; set; }
}
