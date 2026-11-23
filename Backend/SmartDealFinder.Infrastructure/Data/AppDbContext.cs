using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using SmartDealFinder.Domain.Entities;

namespace SmartDealFinder.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Brand> Brands { get; set; } = null!;
    public DbSet<Merchant> Merchants { get; set; } = null!;
    public DbSet<Wishlist> Wishlists { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql(
                "Server=localhost;Port=3306;Database=smart_deal_finder;User=root;Password=@12631263;CharSet=utf8mb4;TreatTinyAsBoolean=false",
                new MySqlServerVersion(new Version(8, 0, 46)));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.CurrentPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().Property(p => p.OriginalPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().Property(p => p.LowestPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().Property(p => p.DiscountPercent).HasPrecision(5, 2);

        modelBuilder.Entity<Category>().Property(c => c.Slug).HasMaxLength(160).IsRequired(false);
    }
}