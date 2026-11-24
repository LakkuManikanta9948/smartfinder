using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartDealFinder.Models.Core;

public sealed class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=smart_deal_finder;User=root";
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseMySQL(connectionString)
            .Options;
        return new DatabaseContext(options);
    }
}
