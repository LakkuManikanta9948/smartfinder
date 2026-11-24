using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models.Core;

namespace SmartDealFinder.UI.Services;

public sealed class LegacySchemaUpgrade(DatabaseContext database, ILogger<LegacySchemaUpgrade> logger)
{
    private static readonly (string Current, string Backup)[] LegacyTables =
    [
        ("categories", "legacy_categories_v1"),
        ("brands", "legacy_brands_v1"),
        ("merchants", "legacy_merchants_v1"),
        ("products", "legacy_products_v1"),
        ("wishlists", "legacy_wishlists_v1")
    ];

    private static readonly IReadOnlyDictionary<string, string[]> RequiredColumns = new Dictionary<string, string[]>
    {
        ["categories"] = ["Id", "Name", "Slug", "IconClass", "ParentId", "Active", "CreatedAt", "UpdatedAt"],
        ["brands"] = ["Id", "Name", "LogoUrl", "Country", "Active", "CreatedAt", "UpdatedAt"],
        ["merchants"] = ["Id", "Name", "Website", "LogoUrl", "Country", "Active", "CreatedAt", "UpdatedAt"],
        ["products"] = ["Id", "Title", "Description", "MerchantSku", "ImageUrl", "Currency", "CurrentPrice", "OriginalPrice", "DiscountPercent", "LowestPrice", "LastPriceUpdate", "Active", "Featured", "CreatedAt", "UpdatedAt", "CategoryId", "MerchantId"],
        ["wishlists"] = ["Id", "UserId", "ProductId", "CreatedAt", "UpdatedAt"]
    };

    public async Task PrepareBeforeMigrationsAsync(CancellationToken cancellationToken)
    {
        var tableNames = await GetTableNamesAsync(cancellationToken);
        var hasMigrationHistory = tableNames.Contains("__EFMigrationsHistory", StringComparer.OrdinalIgnoreCase)
            && await HasAppliedMigrationsAsync(cancellationToken);
        if (hasMigrationHistory)
            return;

        var legacyCount = LegacyTables.Count(table => tableNames.Contains(table.Current, StringComparer.OrdinalIgnoreCase));
        if (legacyCount == 0)
            return;

        if (legacyCount != LegacyTables.Length)
            throw new InvalidOperationException("The database has an incomplete legacy catalog schema. No tables were changed; inspect the schema and back it up before migrating.");

        foreach (var (table, requiredColumns) in RequiredColumns)
        {
            var actualColumns = await GetColumnNamesAsync(table, cancellationToken);
            if (!requiredColumns.All(column => actualColumns.Contains(column, StringComparer.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"The legacy `{table}` table has an unsupported column layout. No tables were changed.");
        }

        var backupNames = LegacyTables.Select(table => table.Backup).ToArray();
        if (backupNames.Any(name => tableNames.Contains(name, StringComparer.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Legacy backup tables already exist while EF migrations are not applied. No tables were changed; inspect the previous migration attempt before continuing.");

        var currentApplicationTables = new[] { "AspNetUsers", "ShoppingPlatforms", "PlatformListings", "ProductPriceHistory" };
        if (currentApplicationTables.Any(name => tableNames.Contains(name, StringComparer.OrdinalIgnoreCase)))
            throw new InvalidOperationException("The database contains a partial SmartCompare schema without migration history. No tables were changed; restore a backup or reconcile the migration state first.");

        var renameList = string.Join(", ", LegacyTables.Select(table => $"`{table.Current}` TO `{table.Backup}`"));
        await database.Database.ExecuteSqlRawAsync($"RENAME TABLE {renameList}", cancellationToken);
        logger.LogInformation("Renamed the legacy catalog tables to *_v1 backups before applying SmartCompare migrations.");
    }

    public async Task ImportLegacyCatalogAsync(CancellationToken cancellationToken)
    {
        var tableNames = await GetTableNamesAsync(cancellationToken);
        if (!LegacyTables.All(table => tableNames.Contains(table.Backup, StringComparer.OrdinalIgnoreCase)))
            return;

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var importedCategories = await database.Database.ExecuteSqlRawAsync(
            "INSERT INTO `categories` (`Id`, `Name`, `Slug`, `IconClass`, `ParentId`, `Active`, `CreatedAt`, `UpdatedAt`, `Status`, `CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`) " +
            "SELECT old.`Id`, old.`Name`, old.`Slug`, old.`IconClass`, old.`ParentId`, old.`Active`, old.`CreatedAt`, old.`UpdatedAt`, 1, NULL, old.`CreatedAt`, NULL, old.`UpdatedAt` " +
            "FROM `legacy_categories_v1` old WHERE NOT EXISTS (SELECT 1 FROM `categories` current WHERE current.`Id` = old.`Id`)", cancellationToken);

        var importedBrands = await database.Database.ExecuteSqlRawAsync(
            "INSERT INTO `brands` (`Id`, `Name`, `LogoUrl`, `Country`, `Active`, `CreatedAt`, `UpdatedAt`, `CategoryId`, `Status`, `CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`) " +
            "SELECT old.`Id`, old.`Name`, old.`LogoUrl`, old.`Country`, old.`Active`, old.`CreatedAt`, old.`UpdatedAt`, NULL, 1, NULL, old.`CreatedAt`, NULL, old.`UpdatedAt` " +
            "FROM `legacy_brands_v1` old WHERE NOT EXISTS (SELECT 1 FROM `brands` current WHERE current.`Id` = old.`Id`)", cancellationToken);

        var importedMerchants = await database.Database.ExecuteSqlRawAsync(
            "INSERT INTO `merchants` (`Id`, `Name`, `Website`, `LogoUrl`, `Country`, `Active`, `CreatedAt`, `UpdatedAt`, `CategoryId`, `Status`, `CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`) " +
            "SELECT old.`Id`, old.`Name`, old.`Website`, old.`LogoUrl`, old.`Country`, old.`Active`, old.`CreatedAt`, old.`UpdatedAt`, " +
            "COALESCE((SELECT MIN(product.`CategoryId`) FROM `legacy_products_v1` product INNER JOIN `categories` category ON category.`Id` = product.`CategoryId` WHERE product.`MerchantId` = old.`Id`), (SELECT MIN(category.`Id`) FROM `categories` category)), " +
            "1, NULL, old.`CreatedAt`, NULL, old.`UpdatedAt` FROM `legacy_merchants_v1` old " +
            "WHERE NOT EXISTS (SELECT 1 FROM `merchants` current WHERE current.`Id` = old.`Id`)", cancellationToken);

        var importedProducts = await database.Database.ExecuteSqlRawAsync(
            "INSERT INTO `products` (`Id`, `Title`, `Description`, `MerchantSku`, `ModelNumber`, `Gtin`, `Variant`, `ImageUrl`, `Currency`, `CurrentPrice`, `OriginalPrice`, `DiscountPercent`, `LowestPrice`, `LastPriceUpdate`, `Active`, `Featured`, `Rating`, `ReviewCount`, `BrandId`, `CreatedAt`, `UpdatedAt`, `CategoryId`, `MerchantId`, `Status`, `CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`) " +
            "SELECT old.`Id`, old.`Title`, old.`Description`, old.`MerchantSku`, NULL, NULL, NULL, old.`ImageUrl`, old.`Currency`, old.`CurrentPrice`, old.`OriginalPrice`, old.`DiscountPercent`, old.`LowestPrice`, old.`LastPriceUpdate`, old.`Active`, old.`Featured`, NULL, NULL, " +
            "(SELECT MIN(brand.`Id`) FROM `legacy_brands_v1` brand WHERE INSTR(LOWER(old.`Title`), LOWER(brand.`Name`)) > 0), " +
            "old.`CreatedAt`, old.`UpdatedAt`, old.`CategoryId`, old.`MerchantId`, 1, NULL, old.`CreatedAt`, NULL, old.`UpdatedAt` " +
            "FROM `legacy_products_v1` old WHERE NOT EXISTS (SELECT 1 FROM `products` current WHERE current.`Id` = old.`Id`)", cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation(
            "Imported legacy catalog records into SmartCompare: {Categories} categories, {Brands} brands, {Merchants} merchants, and {Products} products. Original legacy tables were retained.",
            importedCategories, importedBrands, importedMerchants, importedProducts);
    }

    private async Task<HashSet<string>> GetTableNamesAsync(CancellationToken cancellationToken)
    {
        await database.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = database.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT TABLE_NAME FROM information_schema.tables WHERE TABLE_SCHEMA = DATABASE()";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (await reader.ReadAsync(cancellationToken))
                names.Add(reader.GetString(0));
            return names;
        }
        finally
        {
            await database.Database.CloseConnectionAsync();
        }
    }

    private async Task<HashSet<string>> GetColumnNamesAsync(string tableName, CancellationToken cancellationToken)
    {
        await database.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = database.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT COLUMN_NAME FROM information_schema.columns WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (await reader.ReadAsync(cancellationToken))
                names.Add(reader.GetString(0));
            return names;
        }
        finally
        {
            await database.Database.CloseConnectionAsync();
        }
    }

    private async Task<bool> HasAppliedMigrationsAsync(CancellationToken cancellationToken)
    {
        var result = await database.Database.SqlQueryRaw<long>("SELECT COUNT(*) AS `Value` FROM `__EFMigrationsHistory`")
            .SingleAsync(cancellationToken);
        return result > 0;
    }
}
