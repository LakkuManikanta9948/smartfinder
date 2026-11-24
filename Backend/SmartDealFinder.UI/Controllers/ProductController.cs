using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartDealFinder.Models;
using SmartDealFinder.Service;

namespace SmartDealFinder.UI.Controllers;

[Authorize]
public class ProductController : Controller
{
    private readonly IProductService _service;
    private readonly IAuthorizationService _authorization;

    public ProductController(IProductService service, IAuthorizationService authorization)
    {
        _service = service;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!(await _authorization.AuthorizeAsync(User, PermissionCodes.ProductsView)).Succeeded)
            return Forbid();
        return View(await _service.GetListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Save(int id = 0)
    {
        var permission = id == 0 ? PermissionCodes.ProductsCreate : PermissionCodes.ProductsEdit;
        if (!(await _authorization.AuthorizeAsync(User, permission)).Succeeded)
            return Forbid();
        var model = id == 0 ? new Product() : await _service.GetByIdAsync(id) ?? new Product();
        await SetSelectListsAsync(model.CategoryId, model.MerchantId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Product model)
    {
        var permission = model.Id == 0 ? PermissionCodes.ProductsCreate : PermissionCodes.ProductsEdit;
        if (!(await _authorization.AuthorizeAsync(User, permission)).Succeeded)
            return Forbid();
        if (!ModelState.IsValid)
        {
            await SetSelectListsAsync(model.CategoryId, model.MerchantId);
            return View(model);
        }

        await _service.SaveAsync(model);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!(await _authorization.AuthorizeAsync(User, PermissionCodes.ProductsDelete)).Succeeded)
            return Forbid();
        await _service.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (!(await _authorization.AuthorizeAsync(User, PermissionCodes.ProductsView)).Succeeded)
            return Forbid();
        var model = await _service.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    private async Task SetSelectListsAsync(int categoryId, int merchantId)
    {
        ViewBag.Categories = new SelectList(await _service.GetCategoriesAsync(), "Id", "Name", categoryId);
        ViewBag.Merchants = new SelectList(await _service.GetMerchantsAsync(), "Id", "Name", merchantId);
    }
}

