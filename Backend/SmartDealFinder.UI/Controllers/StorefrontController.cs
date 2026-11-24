using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDealFinder.Service;

namespace SmartDealFinder.UI.Controllers;

[AllowAnonymous]
[Route("catalog")]
public sealed class StorefrontController(IStorefrontService storefront, IUserProductService userProducts) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? query, int? categoryId, decimal? minimumPrice, decimal? maximumPrice, string? sort, int page = 1, int? brandId = null, int? platformId = null, decimal? minimumRating = null, bool? inStock = null, CancellationToken cancellationToken = default)
    {
        if (minimumPrice < 0 || maximumPrice < 0 || minimumPrice.HasValue && maximumPrice.HasValue && minimumPrice > maximumPrice || minimumRating is < 0 or > 5)
            return BadRequest("Search filters are invalid.");

        var result = await storefront.SearchAsync(query, categoryId, minimumPrice, maximumPrice, sort, page, brandId, platformId, minimumRating, inStock, cancellationToken);
        return View("~/Views/Home/Index.cshtml", result);
    }

    [HttpGet("product/{id:int}/{slug?}")]
    public async Task<IActionResult> Details(int id, string? slug, CancellationToken cancellationToken)
    {
        var product = await storefront.GetProductAsync(id, cancellationToken);
        if (product == null)
            return NotFound();

        var canonicalSlug = Slugify(product.Title);
        if (!string.Equals(slug, canonicalSlug, StringComparison.Ordinal))
            return RedirectToActionPermanent(nameof(Details), new { id, slug = canonicalSlug });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
            await userProducts.RecordProductViewAsync(userId, id, cancellationToken);

        ViewBag.RelatedProducts = await storefront.GetRelatedProductsAsync(id, cancellationToken);
        return View("Details", product);
    }

    [HttpGet("product/{id:int}/compare")]
    public async Task<IActionResult> Compare(int id, CancellationToken cancellationToken)
    {
        var comparison = await storefront.CompareAsync(id, cancellationToken);
        return comparison == null ? NotFound() : View("Compare", comparison);
    }

    [HttpGet("product/{id:int}/price-history")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> PriceHistory(int id, CancellationToken cancellationToken)
    {
        if (await storefront.GetProductAsync(id, cancellationToken) == null)
            return NotFound();
        return Json(await storefront.GetPriceHistoryAsync(id, cancellationToken));
    }

    private static string Slugify(string value)
    {
        var slug = string.Concat(value.ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        return string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
