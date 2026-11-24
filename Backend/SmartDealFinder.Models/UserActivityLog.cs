using System.ComponentModel.DataAnnotations;

namespace SmartDealFinder.Models;

public sealed class UserActivityLog
{
    public long Id { get; set; }

    [Required, StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string ActivityType { get; set; } = string.Empty;

    public int? ProductId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
