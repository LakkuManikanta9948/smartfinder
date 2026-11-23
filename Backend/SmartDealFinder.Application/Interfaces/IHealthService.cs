using SmartDealFinder.Application.DTOs;

namespace SmartDealFinder.Application.Interfaces;

public interface IHealthService
{
    Task<HealthDto> GetHealthAsync();
}