using Microsoft.AspNetCore.Identity;

namespace SmartDealFinder.Models;

public sealed class ApplicationRole : IdentityRole
{
    public bool IsSystemRole { get; set; }
}
