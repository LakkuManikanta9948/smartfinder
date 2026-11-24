CREATE DATABASE IF NOT EXISTS `smart_deal_finder` CHARACTER SET utf8mb4;
USE `smart_deal_finder`;

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `AdminActivityLogs` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `ActorUserId` varchar(450) NOT NULL,
    `Action` varchar(100) NOT NULL,
    `Subject` varchar(500) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetRoles` (
    `Id` varchar(255) NOT NULL,
    `IsSystemRole` tinyint(1) NOT NULL,
    `Name` varchar(256) NULL,
    `NormalizedName` varchar(256) NULL,
    `ConcurrencyStamp` longtext NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetUsers` (
    `Id` varchar(255) NOT NULL,
    `DisplayName` longtext NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `UserName` varchar(256) NULL,
    `NormalizedUserName` varchar(256) NULL,
    `Email` varchar(256) NULL,
    `NormalizedEmail` varchar(256) NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext NULL,
    `SecurityStamp` longtext NULL,
    `ConcurrencyStamp` longtext NULL,
    `PhoneNumber` longtext NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `categories` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(120) NOT NULL,
    `Slug` varchar(160) NULL,
    `IconClass` varchar(20) NULL,
    `ParentId` int NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Notifications` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(450) NOT NULL,
    `Title` varchar(160) NOT NULL,
    `Message` varchar(1000) NOT NULL,
    `IsRead` tinyint(1) NOT NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `PermissionAudits` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `ActorUserId` varchar(450) NOT NULL,
    `SubjectUserId` varchar(450) NOT NULL,
    `PermissionCode` varchar(120) NOT NULL,
    `IsGranted` tinyint(1) NOT NULL,
    `ChangedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `PhoneOtpChallenges` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PhoneNumber` varchar(32) NOT NULL,
    `CodeHash` longblob NOT NULL,
    `Salt` longblob NOT NULL,
    `ExpiresAt` datetime(6) NOT NULL,
    `WindowStartedAt` datetime(6) NOT NULL,
    `LastSentAt` datetime(6) NOT NULL,
    `SendsInWindow` int NOT NULL,
    `FailedAttempts` int NOT NULL,
    `IsConsumed` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `RolePermissions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(450) NOT NULL,
    `PermissionCode` varchar(120) NOT NULL,
    `IsGranted` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `ShoppingPlatforms` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(80) NOT NULL,
    `WebsiteUrl` varchar(300) NOT NULL,
    `LogoUrl` varchar(300) NULL,
    `IsEnabled` tinyint(1) NOT NULL,
    `IsDevelopmentProvider` tinyint(1) NOT NULL,
    `LastSuccessfulSyncAt` datetime(6) NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `UserActivityLogs` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `UserId` varchar(450) NOT NULL,
    `ActivityType` varchar(60) NOT NULL,
    `ProductId` int NULL,
    `CreatedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `UserPermissions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(450) NOT NULL,
    `PermissionCode` varchar(120) NOT NULL,
    `IsGranted` tinyint(1) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `UpdatedByUserId` longtext NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(255) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserLogins` (
    `LoginProvider` varchar(255) NOT NULL,
    `ProviderKey` varchar(255) NOT NULL,
    `ProviderDisplayName` longtext NULL,
    `UserId` varchar(255) NOT NULL,
    PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserRoles` (
    `UserId` varchar(255) NOT NULL,
    `RoleId` varchar(255) NOT NULL,
    PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserTokens` (
    `UserId` varchar(255) NOT NULL,
    `LoginProvider` varchar(255) NOT NULL,
    `Name` varchar(255) NOT NULL,
    `Value` longtext NULL,
    PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `brands` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(120) NOT NULL,
    `LogoUrl` varchar(200) NULL,
    `Country` varchar(40) NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CategoryId` int NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_brands_categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `categories` (`Id`)
);

CREATE TABLE `merchants` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(120) NOT NULL,
    `Website` varchar(200) NULL,
    `LogoUrl` varchar(200) NULL,
    `Country` varchar(40) NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CategoryId` int NOT NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_merchants_categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `categories` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `products` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Title` varchar(200) NOT NULL,
    `Description` varchar(2000) NULL,
    `MerchantSku` varchar(40) NULL,
    `ImageUrl` varchar(200) NULL,
    `Currency` varchar(40) NULL,
    `CurrentPrice` decimal(18,2) NOT NULL,
    `OriginalPrice` decimal(18,2) NULL,
    `DiscountPercent` decimal(5,2) NULL,
    `LowestPrice` decimal(18,2) NULL,
    `LastPriceUpdate` datetime(6) NULL,
    `Active` tinyint(1) NOT NULL,
    `Featured` tinyint(1) NOT NULL,
    `Rating` decimal(3,2) NULL,
    `ReviewCount` int NULL,
    `BrandId` int NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CategoryId` int NOT NULL,
    `MerchantId` int NOT NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_products_brands_BrandId` FOREIGN KEY (`BrandId`) REFERENCES `brands` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_products_categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `categories` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_products_merchants_MerchantId` FOREIGN KEY (`MerchantId`) REFERENCES `merchants` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PlatformListings` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProductId` int NOT NULL,
    `ShoppingPlatformId` int NOT NULL,
    `Title` varchar(240) NOT NULL,
    `ProductUrl` varchar(1500) NOT NULL,
    `ImageUrl` varchar(500) NULL,
    `CurrentPrice` decimal(18,2) NOT NULL,
    `OriginalPrice` decimal(18,2) NULL,
    `Currency` varchar(8) NOT NULL,
    `IsAvailable` tinyint(1) NOT NULL,
    `IsVerifiedLiveData` tinyint(1) NOT NULL,
    `DataSource` varchar(40) NOT NULL,
    `LastUpdatedAt` datetime(6) NOT NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PlatformListings_ShoppingPlatforms_ShoppingPlatformId` FOREIGN KEY (`ShoppingPlatformId`) REFERENCES `ShoppingPlatforms` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PlatformListings_products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PriceAlerts` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(450) NOT NULL,
    `ProductId` int NOT NULL,
    `TargetPrice` decimal(18,2) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `LastTriggeredAt` datetime(6) NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PriceAlerts_products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `ProductImages` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProductId` int NOT NULL,
    `ImageUrl` varchar(500) NOT NULL,
    `AltText` varchar(160) NULL,
    `SortOrder` int NOT NULL,
    `IsVerifiedSource` tinyint(1) NOT NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ProductImages_products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `ProductSpecifications` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProductId` int NOT NULL,
    `Name` varchar(120) NOT NULL,
    `Value` varchar(500) NOT NULL,
    `IsVerifiedSource` tinyint(1) NOT NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ProductSpecifications_products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `SavedComparisons` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(450) NOT NULL,
    `ProductId` int NOT NULL,
    `Title` varchar(80) NOT NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_SavedComparisons_products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `wishlists` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(450) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `ProductId` int NOT NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_wishlists_products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `ProductPriceHistory` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PlatformListingId` int NOT NULL,
    `Price` decimal(18,2) NOT NULL,
    `CapturedAt` datetime(6) NOT NULL,
    `IsVerifiedLiveData` tinyint(1) NOT NULL,
    `Status` smallint NOT NULL,
    `CreatedBy` bigint NULL,
    `CreatedOn` datetime(6) NOT NULL,
    `ModifiedBy` bigint NULL,
    `ModifiedOn` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ProductPriceHistory_PlatformListings_PlatformListingId` FOREIGN KEY (`PlatformListingId`) REFERENCES `PlatformListings` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);

CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);

CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);

CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

CREATE INDEX `IX_brands_CategoryId` ON `brands` (`CategoryId`);

CREATE INDEX `IX_merchants_CategoryId` ON `merchants` (`CategoryId`);

CREATE UNIQUE INDEX `IX_PhoneOtpChallenges_PhoneNumber` ON `PhoneOtpChallenges` (`PhoneNumber`);

CREATE INDEX `IX_PlatformListings_ProductId_IsAvailable` ON `PlatformListings` (`ProductId`, `IsAvailable`);

CREATE UNIQUE INDEX `IX_PlatformListings_ProductId_ShoppingPlatformId` ON `PlatformListings` (`ProductId`, `ShoppingPlatformId`);

CREATE INDEX `IX_PlatformListings_ShoppingPlatformId` ON `PlatformListings` (`ShoppingPlatformId`);

CREATE INDEX `IX_PriceAlerts_ProductId` ON `PriceAlerts` (`ProductId`);

CREATE INDEX `IX_PriceAlerts_UserId_ProductId_IsActive` ON `PriceAlerts` (`UserId`, `ProductId`, `IsActive`);

CREATE INDEX `IX_ProductImages_ProductId_SortOrder` ON `ProductImages` (`ProductId`, `SortOrder`);

CREATE INDEX `IX_ProductPriceHistory_PlatformListingId` ON `ProductPriceHistory` (`PlatformListingId`);

CREATE INDEX `IX_products_BrandId` ON `products` (`BrandId`);

CREATE INDEX `IX_products_CategoryId` ON `products` (`CategoryId`);

CREATE INDEX `IX_products_MerchantId` ON `products` (`MerchantId`);

CREATE INDEX `IX_ProductSpecifications_ProductId_Name` ON `ProductSpecifications` (`ProductId`, `Name`);

CREATE UNIQUE INDEX `IX_RolePermissions_RoleId_PermissionCode` ON `RolePermissions` (`RoleId`, `PermissionCode`);

CREATE INDEX `IX_SavedComparisons_ProductId` ON `SavedComparisons` (`ProductId`);

CREATE INDEX `IX_SavedComparisons_UserId_CreatedAt` ON `SavedComparisons` (`UserId`, `CreatedAt`);

CREATE INDEX `IX_UserActivityLogs_UserId_CreatedAt` ON `UserActivityLogs` (`UserId`, `CreatedAt`);

CREATE UNIQUE INDEX `IX_UserPermissions_UserId_PermissionCode` ON `UserPermissions` (`UserId`, `PermissionCode`);

CREATE INDEX `IX_wishlists_ProductId` ON `wishlists` (`ProductId`);

CREATE UNIQUE INDEX `IX_wishlists_UserId_ProductId` ON `wishlists` (`UserId`, `ProductId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261124153458_InitialSmartCompare', '10.0.15');

COMMIT;

