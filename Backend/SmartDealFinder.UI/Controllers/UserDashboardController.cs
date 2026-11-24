using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartDealFinder.Models;
using SmartDealFinder.Service;
using SmartDealFinder.UI.Models;

namespace SmartDealFinder.UI.Controllers;

[Authorize]
[Route("account")]
public sealed class UserDashboardController(
    IUserProductService products,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
            return Challenge();
        ViewBag.DisplayName = (await userManager.GetUserAsync(User))?.DisplayName ?? User.Identity?.Name;
        return View(await products.GetDashboardAsync(userId, cancellationToken));
    }

    [HttpPost("wishlist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToWishlist(int productId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
            return Challenge();
        await products.AddToWishlistAsync(userId, productId, cancellationToken);
        TempData["Success"] = "Product saved to your wishlist.";
        return RedirectToAction("Details", "Storefront", new { id = productId });
    }

    [HttpPost("wishlist/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromWishlist(int productId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
            return Challenge();
        await products.RemoveFromWishlistAsync(userId, productId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("price-alert")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPriceAlert(PriceAlertInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a valid target price greater than zero.";
            return RedirectToAction("Details", "Storefront", new { id = input.ProductId });
        }
        var userId = userManager.GetUserId(User);
        if (userId == null)
            return Challenge();
        await products.AddPriceAlertAsync(userId, input.ProductId, input.TargetPrice, cancellationToken);
        TempData["Success"] = "Price alert saved. You’ll be notified only when verified provider data reaches your target.";
        return RedirectToAction("Details", "Storefront", new { id = input.ProductId });
    }

    [HttpPost("price-alert/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePriceAlert(int alertId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
            return Challenge();
        await products.RemovePriceAlertAsync(userId, alertId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("comparisons/{productId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveComparison(int productId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
            return Challenge();
        await products.SaveComparisonAsync(userId, productId, cancellationToken);
        TempData["Success"] = "Comparison saved to your account.";
        return RedirectToAction("Compare", "Storefront", new { id = productId });
    }
}
