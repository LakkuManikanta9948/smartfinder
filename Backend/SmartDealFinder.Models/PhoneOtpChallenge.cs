using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class PhoneOtpChallenge
{
    public int Id { get; set; }

    [Required, StringLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public byte[] CodeHash { get; set; } = [];

    [Required]
    public byte[] Salt { get; set; } = [];

    public DateTime ExpiresAt { get; set; }
    public DateTime WindowStartedAt { get; set; }
    public DateTime LastSentAt { get; set; }
    public int SendsInWindow { get; set; }
    public int FailedAttempts { get; set; }
    public bool IsConsumed { get; set; }
}
