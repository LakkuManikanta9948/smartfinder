using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Service;
using SmartDealFinder.UI.Models;

namespace SmartDealFinder.UI.Controllers;

[Authorize(Policy = PermissionCodes.PermissionsManage)]
[Route("admin/permissions")]
public sealed class PermissionAdminController(
    DatabaseContext database,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? userId, CancellationToken cancellationToken)
    {
        var users = await GetAssignableUsersAsync(cancellationToken);
        var selectedId = userId;
        if (string.IsNullOrWhiteSpace(selectedId))
            selectedId = users.FirstOrDefault()?.Id;

        var selectedUser = selectedId == null ? null : await userManager.FindByIdAsync(selectedId);
        if (selectedUser != null && !users.Any(item => item.Id == selectedUser.Id))
            return Forbid();

        var grants = selectedId == null
            ? new Dictionary<string, bool>()
            : await database.UserPermissions.AsNoTracking()
                .Where(item => item.UserId == selectedId)
                .ToDictionaryAsync(item => item.PermissionCode, item => item.IsGranted, cancellationToken);

        return View(new PermissionEditorViewModel
        {
            TargetUserId = selectedId ?? string.Empty,
            TargetLabel = selectedUser?.PhoneNumber ?? "Select an account",
            Users = users,
            Permissions = PermissionCodes.All.Select(code => new PermissionToggle
            {
                Code = code,
                Label = code.Replace('.', ' '),
                IsGranted = grants.TryGetValue(code, out var granted) && granted
            }).ToList()
        });
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(PermissionEditorViewModel model, CancellationToken cancellationToken)
    {
        var target = await userManager.FindByIdAsync(model.TargetUserId);
        if (target == null)
            return NotFound();
        if (await userManager.IsInRoleAsync(target, "ORGADMIN"))
            return Forbid();
        if (model.Permissions.Any(item => !PermissionCodes.All.Contains(item.Code, StringComparer.Ordinal)))
            return BadRequest("An unknown permission was submitted.");

        var actorId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(actorId))
            return Challenge();

        var current = await database.UserPermissions.Where(item => item.UserId == target.Id).ToListAsync(cancellationToken);
        foreach (var toggle in model.Permissions)
        {
            var permission = current.SingleOrDefault(item => item.PermissionCode == toggle.Code);
            if (permission?.IsGranted == toggle.IsGranted)
                continue;

            if (permission == null)
            {
                permission = new UserPermission { UserId = target.Id, PermissionCode = toggle.Code };
                database.UserPermissions.Add(permission);
            }

            permission.IsGranted = toggle.IsGranted;
            permission.UpdatedAt = DateTime.UtcNow;
            permission.UpdatedByUserId = actorId;
            database.PermissionAudits.Add(new PermissionAudit
            {
                ActorUserId = actorId,
                SubjectUserId = target.Id,
                PermissionCode = toggle.Code,
                IsGranted = toggle.IsGranted,
                ChangedAt = DateTime.UtcNow
            });
        }

        await database.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Permissions saved. Authorization checks use the updated database grants.";
        return RedirectToAction(nameof(Index), new { userId = target.Id });
    }

    private async Task<IReadOnlyList<PermissionUserOption>> GetAssignableUsersAsync(CancellationToken cancellationToken)
    {
        var orgAdminIds = await userManager.GetUsersInRoleAsync("ORGADMIN");
        var protectedIds = orgAdminIds.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        return await userManager.Users.AsNoTracking()
            .Where(item => !protectedIds.Contains(item.Id))
            .OrderBy(item => item.PhoneNumber)
            .Select(item => new PermissionUserOption { Id = item.Id, Label = item.PhoneNumber ?? item.UserName ?? item.Id })
            .ToListAsync(cancellationToken);
    }
}
