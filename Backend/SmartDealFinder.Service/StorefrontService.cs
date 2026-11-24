using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Models.DTO;
using SmartDealFinder.Models.ViewModels;

namespace SmartDealFinder.Service;

public interface IStorefrontService
{
    Task<CatalogPageViewModel> SearchAsync(string? query, int? categoryId, decimal? minimumPrice, decimal? maximumPrice, string? sort, int page, int? brandId = null, int? platformId = null, decimal? minimumRating = null, bool? inStock = null, CancellationToken cancellationToken = default);
    Task<Product?> GetProductAsync(int id, CancellationToken cancellationToken = default);
    Task<ComparisonPageViewModel?> CompareAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceHistoryPoint>> GetPriceHistoryAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetRelatedProductsAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogSearchSuggestion>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default);
}

public sealed class StorefrontService : IStorefrontService
{
    private const int PageSize = 12;
    private readonly DatabaseContext _db;
    private readonly IReadOnlyList<IPlatformProductProvider> _providers;

    public StorefrontService(DatabaseContext db, IEnumerable<IPlatformProductProvider> providers)
    {
        _db = db;
        _providers = providers.ToList();
    }

    public async Task<CatalogPageViewModel> SearchAsync(
        string? query,
        int? categoryId,
        decimal? minimumPrice,
        decimal? maximumPrice,
        string? sort,
        int page,
        int? brandId = null,
        int? platformId = null,
        decimal? minimumRating = null,
        bool? inStock = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query?.Trim();
        var normalizedSort = sort?.ToLowerInvariant() switch
        {
            "price-low" => "price-low",
            "price-high" => "price-high",
            "newest" => "newest",
            "highest-rated" => "highest-rated",
            "best-deal" => "best-deal",
            _ => "popular"
        };

        IQueryable<Product> products = _db.Products.AsNoTracking()
            .Where(item => item.Status == 1 && item.Active)
            .Include(item => item.Brand)
            .Include(item => item.Category)
            .Include(item => item.PlatformListings);

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            products = products.Where(item => item.Title.Contains(normalizedQuery)
                || (item.Description != null && item.Description.Contains(normalizedQuery))
                || (item.MerchantSku != null && item.MerchantSku.Contains(normalizedQuery))
                || (item.ModelNumber != null && item.ModelNumber.Contains(normalizedQuery))
                || (item.Gtin != null && item.Gtin.Contains(normalizedQuery))
                || (item.Brand != null && item.Brand.Name.Contains(normalizedQuery))
                || item.Specifications.Any(specification => specification.Name.Contains(normalizedQuery) || specification.Value.Contains(normalizedQuery)));
        }

        if (categoryId.HasValue)
            products = products.Where(item => item.CategoryId == categoryId.Value);
        if (brandId.HasValue)
            products = products.Where(item => item.BrandId == brandId.Value);
        if (platformId.HasValue)
            products = products.Where(item => item.PlatformListings.Any(listing => listing.ShoppingPlatformId == platformId.Value && listing.Status == 1));
        if (minimumRating.HasValue)
            products = products.Where(item => item.Rating >= minimumRating.Value);
        if (inStock == true)
            products = products.Where(item => item.PlatformListings.Any(listing => listing.IsAvailable && listing.Status == 1));
        if (minimumPrice.HasValue)
            products = products.Where(item => item.PlatformListings.Where(listing => listing.IsAvailable && listing.IsVerifiedLiveData && listing.Status == 1).Any()
                ? item.PlatformListings.Where(listing => listing.IsAvailable && listing.IsVerifiedLiveData && listing.Status == 1).Min(listing => listing.CurrentPrice) >= minimumPrice.Value
                : item.CurrentPrice >= minimumPrice.Value);
        if (maximumPrice.HasValue)
            products = products.Where(item => item.PlatformListings.Where(listing => listing.IsAvailable && listing.IsVerifiedLiveData && listing.Status == 1).Any()
                ? item.PlatformListings.Where(listing => listing.IsAvailable && listing.IsVerifiedLiveData && listing.Status == 1).Min(listing => listing.CurrentPrice) <= maximumPrice.Value
                : item.CurrentPrice <= maximumPrice.Value);

        products = normalizedSort switch
        {
            "price-low" => products.OrderBy(item => item.PlatformListings.Where(listing => listing.IsVerifiedLiveData && listing.IsAvailable).Select(listing => (decimal?)listing.CurrentPrice).Min() ?? item.CurrentPrice),
            "price-high" => products.OrderByDescending(item => item.PlatformListings.Where(listing => listing.IsVerifiedLiveData && listing.IsAvailable).Select(listing => (decimal?)listing.CurrentPrice).Max() ?? item.CurrentPrice),
            "highest-rated" => products.OrderByDescending(item => item.Rating ?? 0),
            "newest" => products.OrderByDescending(item => item.CreatedAt),
            "best-deal" => products.OrderByDescending(item => item.PlatformListings.Where(listing => listing.IsVerifiedLiveData && listing.IsAvailable && listing.OriginalPrice.HasValue && listing.OriginalPrice > listing.CurrentPrice).Select(listing => (decimal?)((listing.OriginalPrice!.Value - listing.CurrentPrice) / listing.OriginalPrice.Value)).Max() ?? item.DiscountPercent ?? 0),
            _ => products.OrderByDescending(item => item.Featured).ThenBy(item => item.Title)
        };

        var totalItems = await products.CountAsync(cancellationToken);
        var safePage = Math.Max(1, page);
        var items = await products
            .Skip((safePage - 1) * PageSize)
            .Take(PageSize)
            .Select(item => new CatalogProductItem
            {
                Id = item.Id,
                Title = item.Title,
                BrandName = item.Brand == null ? null : item.Brand.Name,
                CategoryName = item.Category == null ? null : item.Category.Name,
                ImageUrl = item.ImageUrl,
                StartingPrice = item.CurrentPrice,
                BestAvailablePrice = item.PlatformListings.Where(listing => listing.IsVerifiedLiveData && listing.IsAvailable)
                    .Select(listing => (decimal?)listing.CurrentPrice).Min() ?? item.CurrentPrice,
                VerifiedDiscountPercent = item.PlatformListings.Where(listing => listing.IsVerifiedLiveData && listing.IsAvailable && listing.OriginalPrice.HasValue && listing.OriginalPrice.Value > listing.CurrentPrice)
                    .Select(listing => (decimal?)((listing.OriginalPrice!.Value - listing.CurrentPrice) / listing.OriginalPrice.Value * 100)).Max(),
                HasVerifiedPrice = item.PlatformListings.Any(listing => listing.IsVerifiedLiveData && listing.IsAvailable)
            })
            .ToListAsync(cancellationToken);

        var categories = await _db.Categories.AsNoTracking()
            .Where(item => item.Status == 1 && item.Active)
            .OrderBy(item => item.Name)
            .Select(item => new CategoryListItem { Id = item.Id, Name = item.Name, Slug = item.Slug, Active = item.Active })
            .ToListAsync(cancellationToken);
        var brands = await _db.Brands.AsNoTracking()
            .Where(item => item.Status == 1 && item.Active)
            .OrderBy(item => item.Name)
            .Select(item => new CatalogFilterOption { Id = item.Id, Name = item.Name })
            .ToListAsync(cancellationToken);
        var platforms = await _db.ShoppingPlatforms.AsNoTracking()
            .Where(item => item.Status == 1 && item.IsEnabled)
            .OrderBy(item => item.Name)
            .Select(item => new CatalogFilterOption { Id = item.Id, Name = item.Name })
            .ToListAsync(cancellationToken);

        return new CatalogPageViewModel
        {
            Products = items,
            Categories = categories,
            Brands = brands,
            Platforms = platforms,
            Query = normalizedQuery,
            CategoryId = categoryId,
            BrandId = brandId,
            PlatformId = platformId,
            MinimumPrice = minimumPrice,
            MaximumPrice = maximumPrice,
            MinimumRating = minimumRating,
            InStock = inStock,
            Sort = normalizedSort,
            Page = safePage,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize)
        };
    }

    public Task<Product?> GetProductAsync(int id, CancellationToken cancellationToken = default)
        => _db.Products.AsNoTracking()
            .Include(item => item.Brand)
            .Include(item => item.Category)
            .Include(item => item.Images.OrderBy(image => image.SortOrder))
            .Include(item => item.Specifications.OrderBy(specification => specification.Name))
            .Include(item => item.PlatformListings).ThenInclude(listing => listing.ShoppingPlatform)
            .FirstOrDefaultAsync(item => item.Id == id && item.Status == 1 && item.Active, cancellationToken);

    public async Task<ComparisonPageViewModel?> CompareAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(productId, cancellationToken);
        if (product == null)
            return null;

        var offers = product.PlatformListings
            .Where(listing => listing.Status == 1)
            .Select(listing => new ComparisonOfferViewModel
            {
                PlatformName = listing.ShoppingPlatform.Name,
                PlatformLogoUrl = listing.ShoppingPlatform.LogoUrl,
                Title = listing.Title,
                ImageUrl = listing.ImageUrl,
                CurrentPrice = listing.CurrentPrice,
                OriginalPrice = listing.OriginalPrice,
                Currency = listing.Currency,
                IsAvailable = listing.IsAvailable,
                IsVerifiedLiveData = listing.IsVerifiedLiveData,
                DataSource = listing.DataSource,
                LastUpdatedAt = listing.LastUpdatedAt,
                SafeProductUrl = IsAllowedUrl(listing.ProductUrl, listing.ShoppingPlatform.WebsiteUrl) ? listing.ProductUrl : null
            })
            .ToList();

        foreach (var provider in _providers)
        {
            var providerListings = await provider.GetListingsAsync(product, cancellationToken);
            offers.AddRange(providerListings.Select(listing => new ComparisonOfferViewModel
            {
                PlatformName = listing.PlatformName,
                Title = listing.Title,
                ImageUrl = listing.ImageUrl,
                CurrentPrice = listing.CurrentPrice,
                OriginalPrice = listing.OriginalPrice,
                Currency = listing.Currency,
                IsAvailable = listing.IsAvailable,
                IsVerifiedLiveData = listing.IsVerifiedLiveData,
                DataSource = listing.DataSource,
                LastUpdatedAt = listing.LastUpdatedAt,
                SafeProductUrl = IsAllowedUrl(listing.ProductUrl, listing.PlatformWebsite) ? listing.ProductUrl : null
            }));
        }

        return new ComparisonPageViewModel { ProductId = product.Id, ProductTitle = product.Title, Offers = offers };
    }

    public async Task<IReadOnlyList<PriceHistoryPoint>> GetPriceHistoryAsync(int productId, CancellationToken cancellationToken = default)
        => await _db.ProductPriceHistory.AsNoTracking()
            .Where(history => history.IsVerifiedLiveData && history.PlatformListing.ProductId == productId)
            .OrderBy(history => history.CapturedAt)
            .Select(history => new PriceHistoryPoint
            {
                PlatformName = history.PlatformListing.ShoppingPlatform.Name,
                Price = history.Price,
                CapturedAt = history.CapturedAt
            })
            .Take(500)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CatalogSearchSuggestion>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        var term = query.Trim();
        if (term.Length < 2)
            return [];

        var productTitles = await _db.Products.AsNoTracking()
            .Where(item => item.Status == 1 && item.Active && item.Title.Contains(term))
            .OrderBy(item => item.Title)
            .Select(item => item.Title)
            .Take(6)
            .ToListAsync(cancellationToken);
        var brandNames = await _db.Brands.AsNoTracking()
            .Where(item => item.Status == 1 && item.Active && item.Name.Contains(term))
            .OrderBy(item => item.Name)
            .Select(item => item.Name)
            .Take(3)
            .ToListAsync(cancellationToken);
        var categoryNames = await _db.Categories.AsNoTracking()
            .Where(item => item.Status == 1 && item.Active && item.Name.Contains(term))
            .OrderBy(item => item.Name)
            .Select(item => item.Name)
            .Take(2)
            .ToListAsync(cancellationToken);

        return productTitles.Select(value => new CatalogSearchSuggestion { Text = value, Type = "Product" })
            .Concat(brandNames.Select(value => new CatalogSearchSuggestion { Text = value, Type = "Brand" }))
            .Concat(categoryNames.Select(value => new CatalogSearchSuggestion { Text = value, Type = "Category" }))
            .GroupBy(item => item.Text, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(10)
            .ToList();
    }

    public async Task<IReadOnlyList<Product>> GetRelatedProductsAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products.AsNoTracking()
            .Where(item => item.Id == productId && item.Status == 1 && item.Active)
            .Select(item => new { item.CategoryId, item.BrandId })
            .FirstOrDefaultAsync(cancellationToken);
        if (product == null)
            return [];

        return await _db.Products.AsNoTracking()
            .Where(item => item.Id != productId && item.Status == 1 && item.Active
                && (item.CategoryId == product.CategoryId || product.BrandId.HasValue && item.BrandId == product.BrandId))
            .Include(item => item.Brand)
            .OrderByDescending(item => item.BrandId == product.BrandId && product.BrandId.HasValue)
            .ThenByDescending(item => item.Featured)
            .Take(6)
            .ToListAsync(cancellationToken);
    }

    private static bool IsAllowedUrl(string? url, string allowedWebsite)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var productUri)
            || !string.Equals(productUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !Uri.TryCreate(allowedWebsite, UriKind.Absolute, out var platformUri))
            return false;

        var host = productUri.Host;
        var allowedHost = platformUri.Host;
        return host.Equals(allowedHost, StringComparison.OrdinalIgnoreCase)
            || host.EndsWith('.' + allowedHost, StringComparison.OrdinalIgnoreCase);
    }
}
