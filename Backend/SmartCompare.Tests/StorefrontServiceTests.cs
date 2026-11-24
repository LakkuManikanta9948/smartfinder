using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Service;

namespace SmartCompare.Tests;

public sealed class StorefrontServiceTests
{
    [Theory]
    [InlineData("https://amazon.in.evil.example/item", null)]
    [InlineData("http://amazon.in/item", null)]
    [InlineData("https://amazon.in/item", "https://amazon.in/item")]
    [InlineData("https://www.amazon.in/item", "https://www.amazon.in/item")]
    public async Task CompareAsync_OnlyReturnsHttpsLinksOnTheConfiguredRetailerHost(string productUrl, string? expectedUrl)
    {
        await using var database = CreateDatabase();
        var category = new Category { Name = "Electronics" };
        var merchant = new Merchant { Name = "Catalog merchant", Category = category };
        var product = new Product
        {
            Title = "Test phone",
            CurrentPrice = 100m,
            Category = category,
            Merchant = merchant
        };
        var platform = new ShoppingPlatform
        {
            Name = "Amazon India",
            WebsiteUrl = "https://amazon.in",
            IsEnabled = true
        };
        database.Products.Add(product);
        database.ShoppingPlatforms.Add(platform);
        database.PlatformListings.Add(new PlatformListing
        {
            Product = product,
            ShoppingPlatform = platform,
            Title = "Test phone listing",
            ProductUrl = productUrl,
            CurrentPrice = 99m,
            IsAvailable = true,
            IsVerifiedLiveData = true,
            DataSource = "Authorized test provider"
        });
        await database.SaveChangesAsync();

        var service = new StorefrontService(database, []);
        var comparison = await service.CompareAsync(product.Id);

        Assert.NotNull(comparison);
        Assert.Single(comparison.Offers);
        Assert.Equal(expectedUrl, comparison.Offers[0].SafeProductUrl);
    }

    [Fact]
    public async Task SearchAsync_AppliesBrandPlatformRatingStockAndVerifiedPriceFilters()
    {
        await using var database = CreateDatabase();
        var category = new Category { Name = "Electronics" };
        var merchant = new Merchant { Name = "Catalog merchant", Category = category };
        var brand = new Brand { Name = "Acme" };
        var platform = new ShoppingPlatform { Name = "Retailer", WebsiteUrl = "https://retailer.example", IsEnabled = true };
        var matching = new Product
        {
            Title = "Acme verified phone",
            CurrentPrice = 900m,
            Rating = 4.5m,
            Brand = brand,
            Category = category,
            Merchant = merchant
        };
        matching.PlatformListings.Add(new PlatformListing
        {
            ShoppingPlatform = platform,
            Title = matching.Title,
            ProductUrl = "https://retailer.example/item",
            CurrentPrice = 450m,
            IsAvailable = true,
            IsVerifiedLiveData = true,
            DataSource = "Authorized test provider"
        });
        database.Products.Add(matching);
        await database.SaveChangesAsync();

        var service = new StorefrontService(database, []);
        var result = await service.SearchAsync(null, category.Id, 400m, 500m, "price-low", 1, brand.Id, platform.Id, 4m, true);

        var product = Assert.Single(result.Products);
        Assert.Equal(matching.Id, product.Id);
        Assert.Equal(450m, product.BestAvailablePrice);
        Assert.True(product.HasVerifiedPrice);
    }

    private static DatabaseContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DatabaseContext(options);
    }
}
