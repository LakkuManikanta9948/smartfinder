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
[Route("admin/permissions/roles")]
public sealed class RolePermissionAdminController(
    DatabaseContext database,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager) : Controller
{
    private static readonly string[] AssignableRoles = ["ADMIN", "USER"];

    [HttpGet("")]
    public async Task<IActionResult> Index(string? roleId, CancellationToken cancellationToken)
    {
        var roles = await roleManager.Roles.AsNoTracking()
            .Where(role => role.Name != null && AssignableRoles.Contains(role.Name))
            .OrderBy(role => role.Name)
            .Select(role => new PermissionUserOption { Id = role.Id, Label = role.Name! })
            .ToListAsync(cancellationToken);
        var selectedRoleId = roleId ?? roles.FirstOrDefault()?.Id;
        if (selectedRoleId != null && !roles.Any(role => role.Id == selectedRoleId))
            return Forbid();
        var selectedRole = selectedRoleId == null ? null : await roleManager.FindByIdAsync(selectedRoleId);
        var grants = selectedRoleId == null
            ? new Dictionary<string, bool>()
            : await database.RolePermissions.AsNoTracking()
                .Where(permission => permission.RoleId == selectedRoleId)
                .ToDictionaryAsync(permission => permission.PermissionCode, permission => permission.IsGranted, cancellationToken);

        return View(new RolePermissionEditorViewModel
        {
            RoleId = selectedRoleId ?? string.Empty,
            RoleName = selectedRole?.Name ?? string.Empty,
            Roles = roles,
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
    public async Task<IActionResult> Save(RolePermissionEditorViewModel model, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(model.RoleId);
        if (role?.Name == null || !AssignableRoles.Contains(role.Name, StringComparer.Ordinal))
            return Forbid();
        if (model.Permissions.Count != PermissionCodes.All.Count
            || model.Permissions.Any(permission => !PermissionCodes.All.Contains(permission.Code, StringComparer.Ordinal))
            || model.Permissions.Select(permission => permission.Code).Distinct(StringComparer.Ordinal).Count() != PermissionCodes.All.Count)
            return BadRequest("The submitted permission matrix is incomplete or contains unknown permissions.");

        var actorId = userManager.GetUserId(User);
        if (actorId == null)
            return Challenge();

        var current = await database.RolePermissions
            .Where(permission => permission.RoleId == role.Id)
            .ToListAsync(cancellationToken);
        foreach (var toggle in model.Permissions)
        {
            var permission = current.SingleOrDefault(item => item.PermissionCode == toggle.Code);
            if (permission?.IsGranted == toggle.IsGranted)
                continue;
            if (permission == null)
            {
                permission = new RolePermission { RoleId = role.Id, PermissionCode = toggle.Code };
                database.RolePermissions.Add(permission);
            }
            permission.IsGranted = toggle.IsGranted;
            database.PermissionAudits.Add(new PermissionAudit
            {
                ActorUserId = actorId,
                SubjectUserId = role.Id,
                PermissionCode = toggle.Code,
                IsGranted = toggle.IsGranted,
                ChangedAt = DateTime.UtcNow
            });
        }

        await database.SaveChangesAsync(cancellationToken);
        TempData["Success"] = $"Permissions updated for the {role.Name} role.";
        return RedirectToAction(nameof(Index), new { roleId = role.Id });
    }
}
