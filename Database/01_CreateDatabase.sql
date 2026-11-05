/* 01_CreateDatabase.sql - PharmaCare: tạo database, bảng, khóa chính/ngoại, CHECK, UNIQUE, index.
   Chạy lại được nhiều lần (sẽ xóa dữ liệu cũ). Ứng dụng cũng tự chạy 01 -> 03 khi khởi động lần đầu. */
IF DB_ID(N'PharmaCareDB') IS NULL CREATE DATABASE PharmaCareDB;
GO
USE PharmaCareDB;
GO
IF OBJECT_ID('dbo.vw_ExpiryAlerts','V') IS NOT NULL DROP VIEW dbo.vw_ExpiryAlerts;
IF OBJECT_ID('dbo.sp_GetExpiryAlerts','P') IS NOT NULL DROP PROCEDURE dbo.sp_GetExpiryAlerts;
IF OBJECT_ID('dbo.ReceiptLines','U') IS NOT NULL DROP TABLE dbo.ReceiptLines;
IF OBJECT_ID('dbo.ReceiptNotes','U') IS NOT NULL DROP TABLE dbo.ReceiptNotes;
IF OBJECT_ID('dbo.InvoiceLines','U') IS NOT NULL DROP TABLE dbo.InvoiceLines;
IF OBJECT_ID('dbo.Invoices','U') IS NOT NULL DROP TABLE dbo.Invoices;
IF OBJECT_ID('dbo.Batches','U') IS NOT NULL DROP TABLE dbo.Batches;
IF OBJECT_ID('dbo.Medicines','U') IS NOT NULL DROP TABLE dbo.Medicines;
IF OBJECT_ID('dbo.Suppliers','U') IS NOT NULL DROP TABLE dbo.Suppliers;
IF OBJECT_ID('dbo.Categories','U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Users','U') IS NOT NULL DROP TABLE dbo.Users;
GO
CREATE TABLE dbo.Users(
 Username VARCHAR(50) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
 PasswordHash VARCHAR(200) NOT NULL,                       -- PBKDF2-SHA256, không lưu mật khẩu thô
 FullName NVARCHAR(100) NOT NULL,
 Role VARCHAR(20) NOT NULL CONSTRAINT CK_Users_Role CHECK(Role IN ('DUOCSI','QUANLYKHO')),
 IsActive BIT NOT NULL CONSTRAINT DF_Users_Active DEFAULT 1
);
CREATE TABLE dbo.Categories(
 CategoryID INT IDENTITY(1,1) PRIMARY KEY,
 CategoryName NVARCHAR(100) NOT NULL UNIQUE,
 Description NVARCHAR(300) NULL
);
CREATE TABLE dbo.Suppliers(
 SupplierID INT IDENTITY(1,1) PRIMARY KEY,
 SupplierName NVARCHAR(160) NOT NULL CONSTRAINT UQ_Suppliers_Name UNIQUE,
 Phone VARCHAR(20) NULL,
 Email VARCHAR(150) NULL,
 Address NVARCHAR(250) NULL
);
CREATE TABLE dbo.Medicines(
 MedicineID INT IDENTITY(1,1) PRIMARY KEY,
 MedicineCode VARCHAR(30) NOT NULL UNIQUE,
 MedicineName NVARCHAR(160) NOT NULL,
 CategoryID INT NULL REFERENCES dbo.Categories(CategoryID),
 ActiveIngredient NVARCHAR(160) NULL,
 Unit NVARCHAR(30) NOT NULL CONSTRAINT DF_Medicines_Unit DEFAULT N'Viên',
 SellingPrice DECIMAL(18,2) NOT NULL CONSTRAINT CK_Medicines_Price CHECK(SellingPrice>0),
 IsPrescription BIT NOT NULL CONSTRAINT DF_Medicines_Rx DEFAULT 0,
 IsActive BIT NOT NULL CONSTRAINT DF_Medicines_Active DEFAULT 1,
 ImageFileName NVARCHAR(260) NULL,                          -- chỉ lưu TÊN file; ảnh nằm trong thư mục Images cạnh file .exe
 CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Medicines_Created DEFAULT SYSDATETIME()
);
CREATE TABLE dbo.Batches(
 BatchID INT IDENTITY(1,1) PRIMARY KEY,
 BatchCode VARCHAR(40) NOT NULL UNIQUE,
 MedicineID INT NOT NULL REFERENCES dbo.Medicines(MedicineID),
 SupplierID INT NULL REFERENCES dbo.Suppliers(SupplierID),
 QuantityReceived INT NOT NULL CHECK(QuantityReceived>=0),
 QuantityRemaining INT NOT NULL CHECK(QuantityRemaining>=0),
 CostPrice DECIMAL(18,2) NOT NULL CHECK(CostPrice>=0),
 SellingPrice DECIMAL(18,2) NOT NULL CHECK(SellingPrice>0),
 ReceivedDate DATE NOT NULL,
 ExpiryDate DATE NOT NULL,
 CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Batches_Created DEFAULT SYSDATETIME(),
 CONSTRAINT CK_Batches_Expiry CHECK(ExpiryDate>=ReceivedDate)
);
CREATE TABLE dbo.Invoices(
 InvoiceID INT IDENTITY(1,1) PRIMARY KEY,
 InvoiceDate DATETIME2 NOT NULL CONSTRAINT DF_Invoices_Date DEFAULT SYSDATETIME(),
 CustomerName NVARCHAR(160) NOT NULL CONSTRAINT DF_Invoices_Customer DEFAULT N'Khách lẻ',
 TotalAmount DECIMAL(18,2) NOT NULL CHECK(TotalAmount>=0),
 CreatedBy VARCHAR(50) NOT NULL CONSTRAINT FK_Invoices_Users REFERENCES dbo.Users(Username)
);
CREATE TABLE dbo.InvoiceLines(
 InvoiceLineID INT IDENTITY(1,1) PRIMARY KEY,
 InvoiceID INT NOT NULL REFERENCES dbo.Invoices(InvoiceID),
 BatchID INT NOT NULL REFERENCES dbo.Batches(BatchID),
 Quantity INT NOT NULL CHECK(Quantity>0),
 UnitPrice DECIMAL(18,2) NOT NULL CHECK(UnitPrice>=0)
);
CREATE TABLE dbo.ReceiptNotes(            -- phiếu nhập
 ReceiptID INT IDENTITY(1,1) PRIMARY KEY,
 ReceiptDate DATETIME2 NOT NULL CONSTRAINT DF_ReceiptNotes_Date DEFAULT SYSDATETIME(),
 SupplierID INT NULL REFERENCES dbo.Suppliers(SupplierID),
 CreatedBy VARCHAR(50) NOT NULL CONSTRAINT FK_ReceiptNotes_Users REFERENCES dbo.Users(Username),
 TotalAmount DECIMAL(18,2) NOT NULL CHECK(TotalAmount>=0)
);
CREATE TABLE dbo.ReceiptLines(            -- chi tiết phiếu nhập
 ReceiptLineID INT IDENTITY(1,1) PRIMARY KEY,
 ReceiptID INT NOT NULL REFERENCES dbo.ReceiptNotes(ReceiptID),
 BatchID INT NOT NULL REFERENCES dbo.Batches(BatchID),
 Quantity INT NOT NULL CHECK(Quantity>0),
 CostPrice DECIMAL(18,2) NOT NULL CHECK(CostPrice>=0)
);
GO
CREATE INDEX IX_Batches_Medicine_Expiry ON dbo.Batches(MedicineID,ExpiryDate) INCLUDE(QuantityRemaining,SellingPrice);
CREATE INDEX IX_Invoices_Date ON dbo.Invoices(InvoiceDate);
GO
