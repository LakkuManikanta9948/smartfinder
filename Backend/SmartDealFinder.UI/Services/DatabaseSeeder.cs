using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;

namespace SmartDealFinder.UI.Services;

public sealed class DatabaseSeeder(
    DatabaseContext database,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<DatabaseSeeder> logger,
    LegacySchemaUpgrade legacySchemaUpgrade)
{
    private static readonly string[] SystemRoles = ["ORGADMIN", "ADMIN", "USER"];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await legacySchemaUpgrade.PrepareBeforeMigrationsAsync(cancellationToken);
        await database.Database.MigrateAsync(cancellationToken);
        await legacySchemaUpgrade.ImportLegacyCatalogAsync(cancellationToken);

        foreach (var roleName in SystemRoles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName, IsSystemRole = true });
            EnsureSucceeded(result, $"Unable to provision the {roleName} system role.");
        }

        await ProvisionOrgAdminAsync();
        if (environment.IsDevelopment() && configuration.GetValue<bool>("DemoData:EnableDevelopmentSampleData"))
            await SeedDevelopmentCatalogAsync(cancellationToken);
    }

    private async Task ProvisionOrgAdminAsync()
    {
        var email = configuration["Bootstrap:OrgAdminEmail"];
        var password = configuration["Bootstrap:OrgAdminPassword"];
        if (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(password))
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
                throw new InvalidOperationException("Configure both Bootstrap:OrgAdminEmail and Bootstrap:OrgAdminPassword with a valid email address.");

            var admin = await userManager.FindByEmailAsync(email.Trim());
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email.Trim(),
                    Email = email.Trim(),
                    EmailConfirmed = true,
                    DisplayName = "Organization Administrator",
                    IsActive = true
                };
                EnsureSucceeded(await userManager.CreateAsync(admin, password), "Unable to provision the initial email ORGADMIN account.");
            }

            if (!await userManager.IsInRoleAsync(admin, "ORGADMIN"))
                EnsureSucceeded(await userManager.AddToRoleAsync(admin, "ORGADMIN"), "Unable to assign the ORGADMIN system role.");
        }

        var phone = configuration["Bootstrap:OrgAdminPhone"];
        if (string.IsNullOrWhiteSpace(phone))
        {
            if (string.IsNullOrWhiteSpace(email))
                logger.LogWarning("No ORGADMIN has been provisioned. Configure Bootstrap__OrgAdminEmail and Bootstrap__OrgAdminPassword securely.");
            return;
        }

        phone = Regex.Replace(phone.Trim(), "[\\s()\\-]", string.Empty, RegexOptions.CultureInvariant);
        if (!Regex.IsMatch(phone, "^\\+[1-9]\\d{7,14}$", RegexOptions.CultureInvariant))
            throw new InvalidOperationException("Bootstrap:OrgAdminPhone must be a valid E.164 phone number.");

        var user = await userManager.FindByNameAsync(phone);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = phone,
                PhoneNumber = phone,
                PhoneNumberConfirmed = true,
                DisplayName = "Organization Administrator",
                IsActive = true
            };
            EnsureSucceeded(await userManager.CreateAsync(user), "Unable to provision the initial phone ORGADMIN account.");
        }

        if (!await userManager.IsInRoleAsync(user, "ORGADMIN"))
            EnsureSucceeded(await userManager.AddToRoleAsync(user, "ORGADMIN"), "Unable to assign the ORGADMIN system role.");
    }

    private async Task SeedDevelopmentCatalogAsync(CancellationToken cancellationToken)
    {
        if (await database.Products.AnyAsync(cancellationToken))
            return;

        var category = new Category { Name = "Electronics", Slug = "electronics", Active = true };
        var brand = new Brand { Name = "Sample brand", Category = category, Active = true };
        var merchant = new Merchant { Name = "Sample retailer", Website = "https://example.com", Category = category, Active = true };
        database.Categories.Add(category);
        database.Brands.Add(brand);
        database.Merchants.Add(merchant);
        await database.SaveChangesAsync(cancellationToken);

        database.Products.AddRange(
            new Product { Title = "Smartphone X — demo catalog item", Description = "Development-only catalogue content. Prices shown by provider are illustrative, not live.", CurrentPrice = 45999m, OriginalPrice = null, Currency = "INR", CategoryId = category.Id, MerchantId = merchant.Id, BrandId = brand.Id, Active = true, Featured = true },
            new Product { Title = "Wireless headphones — demo catalog item", Description = "Development-only catalogue content. Prices shown by provider are illustrative, not live.", CurrentPrice = 7999m, OriginalPrice = null, Currency = "INR", CategoryId = category.Id, MerchantId = merchant.Id, BrandId = brand.Id, Active = true, Featured = true },
            new Product { Title = "Smart watch — demo catalog item", Description = "Development-only catalogue content. Prices shown by provider are illustrative, not live.", CurrentPrice = 12999m, OriginalPrice = null, Currency = "INR", CategoryId = category.Id, MerchantId = merchant.Id, BrandId = brand.Id, Active = true }
        );
        await database.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException($"{message} {string.Join("; ", result.Errors.Select(error => error.Code))}");
    }
}
