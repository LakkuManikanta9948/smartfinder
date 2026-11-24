using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class AppNotification : BaseEntity
{
    [Required, StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }
}
