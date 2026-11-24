using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;

namespace SmartDealFinder.Models.Core;

public sealed class DatabaseContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<ShoppingPlatform> ShoppingPlatforms => Set<ShoppingPlatform>();
    public DbSet<PlatformListing> PlatformListings => Set<PlatformListing>();
    public DbSet<ProductPriceHistory> ProductPriceHistory => Set<ProductPriceHistory>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<PermissionAudit> PermissionAudits => Set<PermissionAudit>();
    public DbSet<PhoneOtpChallenge> PhoneOtpChallenges => Set<PhoneOtpChallenge>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<PriceAlert> PriceAlerts => Set<PriceAlert>();
    public DbSet<SavedComparison> SavedComparisons => Set<SavedComparison>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();
    public DbSet<AdminActivityLog> AdminActivityLogs => Set<AdminActivityLog>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Brand>().Property(e => e.LogoUrl).IsRequired(false);
        modelBuilder.Entity<Brand>().Property(e => e.Country).IsRequired(false);
        modelBuilder.Entity<Category>().Property(e => e.Slug).IsRequired(false);
        modelBuilder.Entity<Category>().Property(e => e.IconClass).IsRequired(false);
        modelBuilder.Entity<Merchant>().Property(e => e.Website).IsRequired(false);
        modelBuilder.Entity<Merchant>().Property(e => e.LogoUrl).IsRequired(false);
        modelBuilder.Entity<Merchant>().Property(e => e.Country).IsRequired(false);
        modelBuilder.Entity<Product>().Property(e => e.Description).IsRequired(false);
        modelBuilder.Entity<Product>().Property(e => e.MerchantSku).IsRequired(false);
        modelBuilder.Entity<Product>().Property(e => e.ImageUrl).IsRequired(false);
        modelBuilder.Entity<Product>().Property(e => e.Currency).IsRequired(false);
        modelBuilder.Entity<Product>().Property(p => p.CurrentPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().Property(p => p.OriginalPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().Property(p => p.LowestPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().Property(p => p.DiscountPercent).HasPrecision(5, 2);
        modelBuilder.Entity<Product>().HasOne(p => p.Brand).WithMany().HasForeignKey(p => p.BrandId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Merchant>().HasOne(merchant => merchant.Category).WithMany()
            .HasForeignKey(merchant => merchant.CategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PlatformListing>().HasIndex(p => new { p.ProductId, p.ShoppingPlatformId }).IsUnique();
        modelBuilder.Entity<PlatformListing>().HasIndex(p => new { p.ProductId, p.IsAvailable });
        modelBuilder.Entity<UserPermission>().HasIndex(p => new { p.UserId, p.PermissionCode }).IsUnique();
        modelBuilder.Entity<RolePermission>().HasIndex(p => new { p.RoleId, p.PermissionCode }).IsUnique();
        modelBuilder.Entity<PhoneOtpChallenge>().HasIndex(p => p.PhoneNumber).IsUnique();
        modelBuilder.Entity<Wishlist>().HasIndex(p => new { p.UserId, p.ProductId }).IsUnique();
        modelBuilder.Entity<PriceAlert>().HasIndex(p => new { p.UserId, p.ProductId, p.IsActive });
        modelBuilder.Entity<SavedComparison>().HasIndex(p => new { p.UserId, p.CreatedAt });
        modelBuilder.Entity<ProductImage>().HasIndex(p => new { p.ProductId, p.SortOrder });
        modelBuilder.Entity<ProductSpecification>().HasIndex(p => new { p.ProductId, p.Name });
        modelBuilder.Entity<UserActivityLog>().HasIndex(p => new { p.UserId, p.CreatedAt });
    }
}
