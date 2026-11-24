using Microsoft.AspNetCore.Mvc;
using SmartDealFinder.Service;

namespace SmartDealFinder.UI.Controllers;

public class HealthController : Controller
{
    private readonly IHealthService _health;

    public HealthController(IHealthService health)
    {
        _health = health;
    }

    public async Task<IActionResult> Index()
    {
        var health = await _health.GetHealthAsync();
        return View(health);
    }
}
