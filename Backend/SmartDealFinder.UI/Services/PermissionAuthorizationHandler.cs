using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Service;

namespace SmartDealFinder.UI.Services;

public sealed record PermissionRequirement(string PermissionCode) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler(
    DatabaseContext database,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return;

        if (context.User.IsInRole("ORGADMIN"))
        {
            context.Succeed(requirement);
            return;
        }

        var userPermission = await database.UserPermissions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.UserId == userId && item.PermissionCode == requirement.PermissionCode);
        if (userPermission != null)
        {
            if (userPermission.IsGranted)
                context.Succeed(requirement);
            return;
        }

        var roleNames = await userManager.GetRolesAsync(new ApplicationUser { Id = userId });
        foreach (var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
                continue;

            var grant = await database.RolePermissions.AsNoTracking()
                .AnyAsync(item => item.RoleId == role.Id && item.PermissionCode == requirement.PermissionCode && item.IsGranted);
            if (grant)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
