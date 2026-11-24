using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.UI.Models;

public sealed class VerifyOtpInput
{
    [Required, StringLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Enter the six-digit verification code.")]
    public string Code { get; set; } = string.Empty;

    [StringLength(2048)]
    public string? ReturnUrl { get; set; }

    public string? Message { get; set; }
    public string? DevelopmentCode { get; set; }
}
