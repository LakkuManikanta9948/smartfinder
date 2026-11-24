using SmartDealFinder.Models.DTO;

namespace SmartDealFinder.Service;

public interface IHealthService
{
    Task<HealthDto> GetHealthAsync();
}
