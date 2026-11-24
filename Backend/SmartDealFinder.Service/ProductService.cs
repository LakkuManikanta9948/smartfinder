using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Models.DTO;

namespace SmartDealFinder.Service;

public interface IProductService
{
    Task<List<ProductListItem>> GetListAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> SaveAsync(Product model);
    Task DeleteAsync(int id);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Merchant>> GetMerchantsAsync();
}

public class ProductService : IProductService
{
    private readonly DatabaseContext _db;

    public ProductService(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<List<ProductListItem>> GetListAsync()
        => await _db.Products
            .Where(e => e.Status == 1)
            .Include(e => e.Category)
            .Include(e => e.Merchant)
            .OrderBy(e => e.Title)
            .Select(e => new ProductListItem
            {
                Id = e.Id,
                Title = e.Title,
                CurrentPrice = e.CurrentPrice,
                DiscountPercent = e.DiscountPercent,
                Active = e.Active,
                CategoryName = e.Category == null ? "" : e.Category.Name,
                MerchantName = e.Merchant == null ? "" : e.Merchant.Name
            })
            .ToListAsync();

    public async Task<Product?> GetByIdAsync(int id)
        => await _db.Products
            .Include(e => e.Category)
            .Include(e => e.Merchant)
            .FirstOrDefaultAsync(e => e.Id == id && e.Status == 1);

    public async Task<Product> SaveAsync(Product model)
    {
        if (model.Id == 0)
        {
            model.Status = 1;
            model.CreatedOn = DateTime.UtcNow;
            _db.Products.Add(model);
        }
        else
        {
            var existing = await _db.Products.FirstOrDefaultAsync(e => e.Id == model.Id && e.Status == 1);
            if (existing == null)
            {
                model.Error = "Record not found";
                return model;
            }

            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.MerchantSku = model.MerchantSku;
            existing.ImageUrl = model.ImageUrl;
            existing.Currency = model.Currency;
            existing.CurrentPrice = model.CurrentPrice;
            existing.OriginalPrice = model.OriginalPrice;
            existing.DiscountPercent = model.DiscountPercent;
            existing.LowestPrice = model.LowestPrice;
            existing.LastPriceUpdate = model.LastPriceUpdate;
            existing.Active = model.Active;
            existing.Featured = model.Featured;
            existing.CategoryId = model.CategoryId;
            existing.MerchantId = model.MerchantId;
            existing.ModifiedOn = DateTime.UtcNow;
            model = existing;
        }

        await _db.SaveChangesAsync();
        return model;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(e => e.Id == id && e.Status == 1);
        if (entity == null)
            return;

        entity.Status = 2;
        entity.ModifiedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<List<Category>> GetCategoriesAsync()
        => await _db.Categories.Where(e => e.Status == 1).OrderBy(e => e.Name).ToListAsync();

    public async Task<List<Merchant>> GetMerchantsAsync()
        => await _db.Merchants.Where(e => e.Status == 1).OrderBy(e => e.Name).ToListAsync();
}
