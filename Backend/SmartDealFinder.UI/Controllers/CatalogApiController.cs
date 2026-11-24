using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDealFinder.Service;

namespace SmartDealFinder.UI.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/catalog")]
public sealed class CatalogApiController(IStorefrontService storefront) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int? categoryId, [FromQuery] decimal? minimumPrice, [FromQuery] decimal? maximumPrice, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int? brandId = null, [FromQuery] int? platformId = null, [FromQuery] decimal? minimumRating = null, [FromQuery] bool? inStock = null, CancellationToken cancellationToken = default)
    {
        if (minimumPrice < 0 || maximumPrice < 0 || minimumPrice.HasValue && maximumPrice.HasValue && minimumPrice > maximumPrice || minimumRating is < 0 or > 5)
            return BadRequest(new { message = "Search filters are invalid." });

        return Ok(await storefront.SearchAsync(query, categoryId, minimumPrice, maximumPrice, sort, page, brandId, platformId, minimumRating, inStock, cancellationToken));
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions([FromQuery] string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2 || query.Length > 100)
            return Ok(Array.Empty<object>());
        return Ok(await storefront.GetSuggestionsAsync(query, cancellationToken));
    }

    [HttpGet("products/{id:int}/comparison")]
    public async Task<IActionResult> Compare(int id, CancellationToken cancellationToken)
    {
        var result = await storefront.CompareAsync(id, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }
}
