using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDealFinder.Models;
using SmartDealFinder.Service;

namespace SmartDealFinder.UI.Controllers;

[Authorize]
public class CategoryController : Controller
{
    private readonly ICategoryService _service;
    private readonly IAuthorizationService _authorization;

    public CategoryController(ICategoryService service, IAuthorizationService authorization)
    {
        _service = service;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!(await _authorization.AuthorizeAsync(User, PermissionCodes.CategoriesView)).Succeeded)
            return Forbid();
        return View(await _service.GetListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Save(int id = 0)
    {
        if (!(await _authorization.AuthorizeAsync(User, PermissionCodes.CategoriesManage)).Succeeded)
            return Forbid();
        var model = id == 0 ? new Category() : await _service.GetByIdAsync(id) ?? new Category();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Category model)
    {
        if (!(await _authorization.AuthorizeAsync(User, PermissionCodes.CategoriesManage)).Succeeded)
            return Forbid();
        if (!ModelState.IsValid)
            return View(model);

        await _service.SaveAsync(model);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!(await _authorization.AuthorizeAsync(User, PermissionCodes.CategoriesManage)).Succeeded)
            return Forbid();
        await _service.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}

