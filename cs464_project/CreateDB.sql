-- Tạo lại database QLCH1 từ EDMX schema
-- Dùng được trên cả LocalDB và Docker SQL Server 2019

-- Tạo database mới (file .mdf sẽ nằm trong thư mục mặc định của LocalDB)
IF DB_ID('QLCH1') IS NOT NULL
BEGIN
    ALTER DATABASE [QLCH1] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [QLCH1];
END
GO

CREATE DATABASE [QLCH1];
GO

USE [QLCH1];
GO

-- ============ TABLES ============

CREATE TABLE [dbo].[Roles] (
    [RoleId]   INT            IDENTITY(1,1) NOT NULL,
    [RoleCode] NVARCHAR(50)   NOT NULL,
    [RoleName] NVARCHAR(100)  NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([RoleId])
);
GO

CREATE TABLE [dbo].[Users] (
    [UserId]       INT            IDENTITY(1,1) NOT NULL,
    [Username]     NVARCHAR(50)   NOT NULL,
    [PasswordHash] VARBINARY(64)  NOT NULL,
    [Salt]         VARBINARY(32)  NOT NULL,
    [FullName]     NVARCHAR(150)  NULL,
    [Email]        NVARCHAR(150)  NULL,
    [Phone]        NVARCHAR(20)   NULL,
    [RoleId]       INT            NOT NULL,
    [IsActive]     BIT            NOT NULL,
    [CreatedAt]    DATETIME2(7)   NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId]),
    CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([RoleId])
);
GO

CREATE TABLE [dbo].[Categories] (
    [CategoryId]   INT            IDENTITY(1,1) NOT NULL,
    [CategoryName] NVARCHAR(150)  NOT NULL,
    [Description]  NVARCHAR(255)  NULL,
    [IsActive]     BIT            NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([CategoryId])
);
GO

CREATE TABLE [dbo].[Customers] (
    [CustomerId]   INT            IDENTITY(1,1) NOT NULL,
    [CustomerCode] NVARCHAR(30)   NOT NULL,
    [FullName]     NVARCHAR(150)  NOT NULL,
    [Phone]        NVARCHAR(20)   NULL,
    [Email]        NVARCHAR(150)  NULL,
    [Address]      NVARCHAR(255)  NULL,
    [CreatedAt]    DATETIME2(7)   NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([CustomerId])
);
GO

CREATE TABLE [dbo].[Employees] (
    [EmployeeId]   INT            IDENTITY(1,1) NOT NULL,
    [UserId]       INT            NULL,
    [EmployeeCode] NVARCHAR(30)   NOT NULL,
    [FullName]     NVARCHAR(150)  NOT NULL,
    [Gender]       NVARCHAR(10)   NULL,
    [BirthDate]    DATE           NULL,
    [Address]      NVARCHAR(255)  NULL,
    [HireDate]     DATE           NULL,
    [BaseSalary]   DECIMAL(18,2)  NOT NULL,
    [IsWorking]    BIT            NOT NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([EmployeeId]),
    CONSTRAINT [FK_Employees_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
);
GO

CREATE TABLE [dbo].[Products] (
    [ProductId]   INT            IDENTITY(1,1) NOT NULL,
    [ProductCode] NVARCHAR(50)   NOT NULL,
    [ProductName] NVARCHAR(200)  NOT NULL,
    [CategoryId]  INT            NOT NULL,
    [Unit]        NVARCHAR(50)   NULL,
    [Price]       DECIMAL(18,2)  NOT NULL,
    [Cost]        DECIMAL(18,2)  NULL,
    [Quantity]    INT            NOT NULL,
    [Description] NVARCHAR(255)  NULL,
    [IsActive]    BIT            NOT NULL,
    [CreatedAt]   DATETIME2(7)   NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ProductId]),
    CONSTRAINT [FK_Products_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([CategoryId])
);
GO

CREATE TABLE [dbo].[Orders] (
    [OrderId]     INT            IDENTITY(1,1) NOT NULL,
    [OrderCode]   NVARCHAR(30)   NOT NULL,
    [CustomerId]  INT            NULL,
    [EmployeeId]  INT            NULL,
    [OrderDate]   DATETIME2(7)   NOT NULL,
    [Status]      NVARCHAR(30)   NOT NULL,
    [Note]        NVARCHAR(255)  NULL,
    [TotalAmount] DECIMAL(18,2)  NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([OrderId]),
    CONSTRAINT [FK_Orders_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([CustomerId]),
    CONSTRAINT [FK_Orders_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([EmployeeId])
);
GO

CREATE TABLE [dbo].[OrderItems] (
    [OrderItemId] INT            IDENTITY(1,1) NOT NULL,
    [OrderId]     INT            NOT NULL,
    [ProductId]   INT            NOT NULL,
    [Quantity]    INT            NOT NULL,
    [UnitPrice]   DECIMAL(18,2)  NOT NULL,
    [LineTotal]   AS ([Quantity] * [UnitPrice]),
    CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([OrderItemId]),
    CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([OrderId]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId])
);
GO

CREATE TABLE [dbo].[StockMovements] (
    [MovementId]   INT            IDENTITY(1,1) NOT NULL,
    [ProductId]    INT            NOT NULL,
    [MovementType] NVARCHAR(10)   NOT NULL,
    [Quantity]     INT            NOT NULL,
    [RefType]      NVARCHAR(30)   NULL,
    [RefId]        INT            NULL,
    [CreatedAt]    DATETIME2(7)   NOT NULL,
    [Note]         NVARCHAR(255)  NULL,
    CONSTRAINT [PK_StockMovements] PRIMARY KEY CLUSTERED ([MovementId]),
    CONSTRAINT [FK_StockMovements_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId])
);
GO

-- ============ SEED DATA ============

-- Thêm Roles mặc định
INSERT INTO [dbo].[Roles] ([RoleCode], [RoleName]) VALUES (N'ADMIN', N'Quản trị viên');
INSERT INTO [dbo].[Roles] ([RoleCode], [RoleName]) VALUES (N'EMPLOYEE', N'Nhân viên');
GO

-- Thêm tài khoản admin mặc định (password: admin123)
-- PasswordHash và Salt được tạo sẵn cho mật khẩu "admin123"
DECLARE @salt VARBINARY(32) = CRYPT_GEN_RANDOM(32);
DECLARE @hash VARBINARY(32) = HASHBYTES('SHA2_256', @salt + CAST('123456' AS VARBINARY(MAX)));

INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Salt], [FullName], [Email], [Phone], [RoleId], [IsActive], [CreatedAt])
VALUES (N'admin', @hash, @salt, N'Administrator', N'admin@qlch.com', N'0123456789', 1, 1, GETDATE());
GO

-- Thêm một số danh mục mẫu
INSERT INTO [dbo].[Categories] ([CategoryName], [Description], [IsActive]) VALUES (N'Thực phẩm', N'Các loại thực phẩm', 1);
INSERT INTO [dbo].[Categories] ([CategoryName], [Description], [IsActive]) VALUES (N'Đồ uống', N'Các loại đồ uống', 1);
INSERT INTO [dbo].[Categories] ([CategoryName], [Description], [IsActive]) VALUES (N'Gia dụng', N'Đồ gia dụng', 1);
GO

PRINT N'Database QLCH1 đã được tạo thành công!';
GO
