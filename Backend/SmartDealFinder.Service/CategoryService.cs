using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Models.DTO;

namespace SmartDealFinder.Service;

public interface ICategoryService
{
    Task<List<CategoryListItem>> GetListAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category> SaveAsync(Category model);
    Task DeleteAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly DatabaseContext _db;

    public CategoryService(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryListItem>> GetListAsync()
        => await _db.Categories
            .Where(e => e.Status == 1)
            .OrderBy(e => e.Name)
            .Select(e => new CategoryListItem
            {
                Id = e.Id,
                Name = e.Name,
                Slug = e.Slug,
                Active = e.Active
            })
            .ToListAsync();

    public async Task<Category?> GetByIdAsync(int id)
        => await _db.Categories
            .Include(e => e.Products)
            .FirstOrDefaultAsync(e => e.Id == id && e.Status == 1);

    public async Task<Category> SaveAsync(Category model)
    {
        if (model.Id == 0)
        {
            model.Status = 1;
            model.CreatedOn = DateTime.UtcNow;
            _db.Categories.Add(model);
        }
        else
        {
            var existing = await _db.Categories.FirstOrDefaultAsync(e => e.Id == model.Id && e.Status == 1);
            if (existing == null)
            {
                model.Error = "Record not found";
                return model;
            }

            existing.Name = model.Name;
            existing.Slug = model.Slug;
            existing.IconClass = model.IconClass;
            existing.ParentId = model.ParentId;
            existing.Active = model.Active;
            existing.ModifiedOn = DateTime.UtcNow;
            model = existing;
        }

        await _db.SaveChangesAsync();
        return model;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(e => e.Id == id && e.Status == 1);
        if (entity == null)
            return;

        entity.Status = 2;
        entity.ModifiedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
