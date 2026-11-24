using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartDealFinder.Service;
using SmartDealFinder.UI.Models;

namespace SmartDealFinder.UI.Controllers;

public class HomeController(IStorefrontService storefront) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? query, int? categoryId, decimal? minimumPrice, decimal? maximumPrice, string? sort, int page = 1, int? brandId = null, int? platformId = null, decimal? minimumRating = null, bool? inStock = null, CancellationToken cancellationToken = default)
    {
        if (minimumPrice < 0 || maximumPrice < 0 || minimumPrice.HasValue && maximumPrice.HasValue && minimumPrice > maximumPrice || minimumRating is < 0 or > 5)
            return BadRequest("Search filters are invalid.");

        return View(await storefront.SearchAsync(query, categoryId, minimumPrice, maximumPrice, sort, page, brandId, platformId, minimumRating, inStock, cancellationToken));
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

