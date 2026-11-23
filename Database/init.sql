-- SmartDealFinder Database Initialization Script
-- Database: smart_deal_finder
-- Credentials: root / @12631263
-- Run this script in MySQL Workbench against localhost:3306

CREATE DATABASE IF NOT EXISTS smart_deal_finder
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE smart_deal_finder;

-- Categories
CREATE TABLE IF NOT EXISTS Categories (
    Id INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(120) NOT NULL,
    Slug VARCHAR(160) NULL,
    IconClass VARCHAR(20) NULL,
    ParentId INT NULL,
    Active TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY UQ_Categories_Slug (Slug),
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentId) REFERENCES Categories(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Brands
CREATE TABLE IF NOT EXISTS Brands (
    Id INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(120) NOT NULL,
    LogoUrl VARCHAR(200) NULL,
    Country VARCHAR(40) NULL,
    Active TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Merchants
CREATE TABLE IF NOT EXISTS Merchants (
    Id INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(120) NOT NULL,
    Website VARCHAR(200) NULL,
    LogoUrl VARCHAR(200) NULL,
    Country VARCHAR(40) NULL,
    Active TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Products
CREATE TABLE IF NOT EXISTS Products (
    Id INT NOT NULL AUTO_INCREMENT,
    Title VARCHAR(200) NOT NULL,
    Description VARCHAR(2000) NULL,
    MerchantSku VARCHAR(40) NULL,
    ImageUrl VARCHAR(200) NULL,
    Currency VARCHAR(40) NOT NULL DEFAULT 'INR',
    CurrentPrice DECIMAL(18,2) NOT NULL,
    OriginalPrice DECIMAL(18,2) NULL,
    DiscountPercent DECIMAL(5,2) NULL,
    LowestPrice DECIMAL(18,2) NULL,
    LastPriceUpdate DATETIME(6) NULL,
    Active TINYINT(1) NOT NULL DEFAULT 1,
    Featured TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    CategoryId INT NOT NULL,
    MerchantId INT NOT NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT FK_Products_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Wishlists
CREATE TABLE IF NOT EXISTS Wishlists (
    Id INT NOT NULL AUTO_INCREMENT,
    UserId VARCHAR(120) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    ProductId INT NOT NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_Wishlists_Product FOREIGN KEY (ProductId) REFERENCES Products(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Seed sample data
INSERT INTO Categories (Name, Slug, IconClass, Active) VALUES
('Electronics', 'electronics', 'bi-laptop', 1),
('Fashion', 'fashion', 'bi-shirt', 1),
('Home & Kitchen', 'home-kitchen', 'bi-house', 1);

INSERT INTO Brands (Name, LogoUrl, Country, Active) VALUES
('Samsung', NULL, 'South Korea', 1),
('Apple', NULL, 'USA', 1),
('Nike', NULL, 'USA', 1);

INSERT INTO Merchants (Name, Website, LogoUrl, Country, Active) VALUES
('Amazon India', 'https://amazon.in', NULL, 'India', 1),
('Flipkart', 'https://flipkart.com', NULL, 'India', 1);

INSERT INTO Products (Title, Description, Currency, CurrentPrice, OriginalPrice, DiscountPercent, Active, Featured, CategoryId, MerchantId) VALUES
('Samsung Galaxy S24', 'Flagship smartphone', 'INR', 74999.00, 79999.00, 6.25, 1, 1, 1, 1),
('Apple MacBook Air M3', 'Laptop with M3 chip', 'INR', 114900.00, 124900.00, 8.00, 1, 1, 1, 2),
('Nike Air Max', 'Running shoes', 'INR', 9999.00, 12999.00, 23.07, 1, 0, 2, 1);

SELECT 'Database smart_deal_finder initialized successfully' AS Message;
