namespace SmartDealFinder.Application.DTOs;

public class HealthDto
{
    public bool DatabaseConnected { get; set; }
    public string Status { get; set; } = "Healthy";
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}