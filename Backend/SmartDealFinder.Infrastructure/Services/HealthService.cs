using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Application.DTOs;
using SmartDealFinder.Application.Interfaces;
using SmartDealFinder.Infrastructure.Data;

namespace SmartDealFinder.Infrastructure.Services;

public class HealthService : IHealthService
{
    private readonly AppDbContext _db;

    public HealthService(AppDbContext db)
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