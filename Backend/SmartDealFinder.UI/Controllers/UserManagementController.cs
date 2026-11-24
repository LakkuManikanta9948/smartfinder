using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Service;
using SmartDealFinder.UI.Models;

namespace SmartDealFinder.UI.Controllers;

[Authorize(Policy = PermissionCodes.UsersManage)]
[Route("admin/users")]
public sealed class UserManagementController(
    DatabaseContext database,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        var orgAdminIds = (await userManager.GetUsersInRoleAsync("ORGADMIN"))
            .Select(user => user.Id)
            .ToHashSet(StringComparer.Ordinal);
        var query = userManager.Users.AsNoTracking().Where(user => !orgAdminIds.Contains(user.Id));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user => (user.PhoneNumber != null && user.PhoneNumber.Contains(term)) || user.DisplayName.Contains(term));
        }

        var users = await query.OrderBy(user => user.PhoneNumber).Take(100).ToListAsync(cancellationToken);
        var userIds = users.Select(user => user.Id).ToList();
        var roles = await (from membership in database.UserRoles.AsNoTracking()
                           join role in database.Roles.AsNoTracking() on membership.RoleId equals role.Id
                           where userIds.Contains(membership.UserId)
                           select new { membership.UserId, role.Name })
            .ToListAsync(cancellationToken);

        return View(new UserManagementViewModel
        {
            Search = search,
            Users = users.Select(user => new ManagedUserItem
            {
                Id = user.Id,
                PhoneNumber = user.PhoneNumber ?? user.UserName ?? string.Empty,
                DisplayName = user.DisplayName,
                Role = roles.FirstOrDefault(role => role.UserId == user.Id && (role.Name == "ADMIN" || role.Name == "USER"))?.Name ?? "USER",
                IsActive = user.IsActive
            }).ToList()
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateManagedUserInput input, CancellationToken cancellationToken)
    {
        if (input.Role is not ("ADMIN" or "USER"))
            ModelState.AddModelError(nameof(input.Role), "Only USER or ADMIN can be assigned here.");
        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        var phone = Regex.Replace(input.PhoneNumber.Trim(), "[\\s()\\-]", string.Empty, RegexOptions.CultureInvariant);
        if (await userManager.FindByNameAsync(phone) != null)
        {
            TempData["Error"] = "An account already exists for that phone number.";
            return RedirectToAction(nameof(Index));
        }

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var user = new ApplicationUser
        {
            UserName = phone,
            PhoneNumber = phone,
            DisplayName = input.DisplayName.Trim(),
            PhoneNumberConfirmed = false,
            IsActive = true
        };
        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["Error"] = string.Join(" ", createResult.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Index));
        }

        var roleResult = await userManager.AddToRoleAsync(user, input.Role);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["Error"] = string.Join(" ", roleResult.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Index));
        }

        AddAuditEntry(user.Id, $"Created {input.Role} account", "Provisioned account");
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        TempData["Success"] = $"{input.Role} account created. The user must verify this phone with OTP before signing in.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(string userId, string role, CancellationToken cancellationToken)
    {
        if (role is not ("ADMIN" or "USER"))
            return BadRequest("Only USER or ADMIN can be assigned here.");
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();
        if (await userManager.IsInRoleAsync(user, "ORGADMIN"))
            return Forbid();

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains(role, StringComparer.Ordinal))
            return RedirectToAction(nameof(Index));

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var managedRoles = currentRoles.Where(name => name is "ADMIN" or "USER").ToArray();
        if (managedRoles.Length > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, managedRoles);
            if (!removeResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                TempData["Error"] = string.Join(" ", removeResult.Errors.Select(error => error.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        var addResult = await userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["Error"] = string.Join(" ", addResult.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Index));
        }

        var stampResult = await userManager.UpdateSecurityStampAsync(user);
        if (!stampResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            TempData["Error"] = string.Join(" ", stampResult.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Index));
        }

        AddAuditEntry(user.Id, "Changed account role", $"Assigned {role}");
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        TempData["Success"] = "Account role updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string userId, bool isActive, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();
        if (await userManager.IsInRoleAsync(user, "ORGADMIN") || user.Id == userManager.GetUserId(User))
            return Forbid();

        user.IsActive = isActive;
        var updateResult = await userManager.UpdateSecurityStampAsync(user);
        if (!updateResult.Succeeded)
            return BadRequest(string.Join(" ", updateResult.Errors.Select(error => error.Description)));

        AddAuditEntry(user.Id, isActive ? "Activated account" : "Deactivated account", "Account status changed");
        await database.SaveChangesAsync(cancellationToken);
        TempData["Success"] = isActive ? "Account activated." : "Account deactivated; active sessions will be revoked on security-stamp validation.";
        return RedirectToAction(nameof(Index));
    }

    private void AddAuditEntry(string subjectUserId, string action, string subject)
    {
        var actorId = userManager.GetUserId(User);
        if (actorId == null)
            throw new InvalidOperationException("An authenticated actor is required for account administration.");

        database.AdminActivityLogs.Add(new AdminActivityLog
        {
            ActorUserId = actorId,
            Action = action,
            Subject = $"{subjectUserId}: {subject}"
        });
    }
}
