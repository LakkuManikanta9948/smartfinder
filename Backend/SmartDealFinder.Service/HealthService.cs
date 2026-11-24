using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Models.DTO;

namespace SmartDealFinder.Service;

public class HealthService : IHealthService
{
    private readonly DatabaseContext _db;

    public HealthService(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<HealthDto> GetHealthAsync()
    {
        var dbConnected = await _db.Database.CanConnectAsync();
        return new HealthDto
        {
            DatabaseConnected = dbConnected,
            Status = dbConnected ? "Healthy" : "Degraded"
        };
    }
}
