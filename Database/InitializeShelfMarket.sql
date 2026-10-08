SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.ShelfRenter', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShelfRenter
    (
        ShelfRenterID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_ShelfRenter PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        UseCustomRent BIT NOT NULL
            CONSTRAINT DF_ShelfRenter_UseCustomRent DEFAULT (0),
        CustomRentPerShelf DECIMAL(10,2) NULL,
        CONSTRAINT CK_ShelfRenter_CustomRent CHECK
        (
            (UseCustomRent = 0 AND CustomRentPerShelf IS NULL)
            OR (UseCustomRent = 1 AND CustomRentPerShelf > 0)
        )
    );
END;
GO

IF OBJECT_ID(N'dbo.Shelf', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shelf
    (
        ShelfID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Shelf PRIMARY KEY,
        Number INT NULL,
        ShelfStatus NVARCHAR(20) NULL,
        Location NVARCHAR(100) NULL,
        NumberOfShelves INT NULL,
        NumberOfClothingRails INT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Employee', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employee
    (
        EmployeeID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Employee PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.RentalAgreement', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RentalAgreement
    (
        RentalAgreementID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_RentalAgreement PRIMARY KEY,
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        Price DECIMAL(10,2) NOT NULL,
        ShelfRenterID INT NOT NULL,
        ShelfID INT NOT NULL,
        PaymentMethod NVARCHAR(20) NOT NULL
            CONSTRAINT DF_RentalAgreement_PaymentMethod DEFAULT ('Cash'),
        CONSTRAINT FK_RentalAgreement_ShelfRenter
            FOREIGN KEY (ShelfRenterID) REFERENCES dbo.ShelfRenter(ShelfRenterID),
        CONSTRAINT FK_RentalAgreement_Shelf
            FOREIGN KEY (ShelfID) REFERENCES dbo.Shelf(ShelfID)
    );
END;
GO

IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Product
    (
        ProductID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Product PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Price DECIMAL(10,2) NOT NULL,
        Barcode NVARCHAR(100) NOT NULL,
        StockQuantity INT NOT NULL,
        ShelfID INT NOT NULL,
        CONSTRAINT FK_Product_Shelf
            FOREIGN KEY (ShelfID) REFERENCES dbo.Shelf(ShelfID)
    );
END;
GO

IF OBJECT_ID(N'dbo.Sale', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sale
    (
        SaleID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Sale PRIMARY KEY,
        SaleDate DATE NOT NULL,
        TotalAmount DECIMAL(10,2) NOT NULL,
        EmployeeID INT NOT NULL,
        PaymentMethod NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Sale_PaymentMethod DEFAULT ('Cash'),
        CONSTRAINT FK_Sale_Employee
            FOREIGN KEY (EmployeeID) REFERENCES dbo.Employee(EmployeeID)
    );
END;
GO

IF OBJECT_ID(N'dbo.SaleLine', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaleLine
    (
        SaleLineID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_SaleLine PRIMARY KEY,
        Quantity INT NOT NULL,
        SalePrice DECIMAL(10,2) NOT NULL,
        ProductID INT NOT NULL,
        SaleID INT NOT NULL,
        ShelfID INT NOT NULL,
        Comment NVARCHAR(100) NULL,
        CONSTRAINT FK_SaleLine_Product
            FOREIGN KEY (ProductID) REFERENCES dbo.Product(ProductID),
        CONSTRAINT FK_SaleLine_Sale
            FOREIGN KEY (SaleID) REFERENCES dbo.Sale(SaleID),
        CONSTRAINT FK_SaleLine_Shelf
            FOREIGN KEY (ShelfID) REFERENCES dbo.Shelf(ShelfID)
    );
END;
GO

IF OBJECT_ID(N'dbo.ProductReturn', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductReturn
    (
        ProductReturnID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_ProductReturn PRIMARY KEY,
        ReturnDate DATE NOT NULL,
        Quantity INT NOT NULL,
        Amount DECIMAL(10,2) NOT NULL,
        Comment NVARCHAR(100) NULL,
        SaleLineID INT NOT NULL,
        CONSTRAINT FK_ProductReturn_SaleLine
            FOREIGN KEY (SaleLineID) REFERENCES dbo.SaleLine(SaleLineID)
    );
END;
GO

IF OBJECT_ID(N'dbo.Settlement', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Settlement
    (
        SettlementID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Settlement PRIMARY KEY,
        SettlementDate DATE NOT NULL,
        TotalSales DECIMAL(10,2) NOT NULL,
        TotalReturns DECIMAL(10,2) NOT NULL
            CONSTRAINT DF_Settlement_TotalReturns DEFAULT (0),
        Commission DECIMAL(10,2) NOT NULL,
        TotalRent DECIMAL(10,2) NOT NULL,
        Result DECIMAL(10,2) NOT NULL,
        ShelfRenterID INT NOT NULL,
        LastModifiedAt DATETIME2(0) NULL,
        ModifiedByEmployeeID INT NULL,
        CONSTRAINT FK_Settlement_ShelfRenter
            FOREIGN KEY (ShelfRenterID) REFERENCES dbo.ShelfRenter(ShelfRenterID),
        CONSTRAINT FK_Settlement_ModifiedByEmployee
            FOREIGN KEY (ModifiedByEmployeeID) REFERENCES dbo.Employee(EmployeeID)
    );
END;
GO

-- Add newer fields to databases that were created by an earlier project version.
IF COL_LENGTH('dbo.ShelfRenter', 'UseCustomRent') IS NULL
BEGIN
    ALTER TABLE dbo.ShelfRenter
    ADD UseCustomRent BIT NOT NULL
        CONSTRAINT DF_ShelfRenter_UseCustomRent DEFAULT (0);
END;
GO

IF COL_LENGTH('dbo.ShelfRenter', 'CustomRentPerShelf') IS NULL
BEGIN
    ALTER TABLE dbo.ShelfRenter ADD CustomRentPerShelf DECIMAL(10,2) NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_ShelfRenter_CustomRent'
      AND parent_object_id = OBJECT_ID('dbo.ShelfRenter')
)
BEGIN
    ALTER TABLE dbo.ShelfRenter
    ADD CONSTRAINT CK_ShelfRenter_CustomRent CHECK
    (
        (UseCustomRent = 0 AND CustomRentPerShelf IS NULL)
        OR (UseCustomRent = 1 AND CustomRentPerShelf > 0)
    );
END;
GO

IF COL_LENGTH('dbo.RentalAgreement', 'PaymentMethod') IS NULL
BEGIN
    ALTER TABLE dbo.RentalAgreement
    ADD PaymentMethod NVARCHAR(20) NOT NULL
        CONSTRAINT DF_RentalAgreement_PaymentMethod DEFAULT ('Cash');
END;
GO

IF COL_LENGTH('dbo.Sale', 'PaymentMethod') IS NULL
BEGIN
    ALTER TABLE dbo.Sale
    ADD PaymentMethod NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Sale_PaymentMethod DEFAULT ('Cash');
END;
GO

IF COL_LENGTH('dbo.SaleLine', 'ShelfID') IS NULL
BEGIN
    ALTER TABLE dbo.SaleLine ADD ShelfID INT NULL;
END;
GO

EXEC sys.sp_executesql N'
    UPDATE sl
    SET ShelfID = p.ShelfID
    FROM dbo.SaleLine AS sl
    INNER JOIN dbo.Product AS p ON p.ProductID = sl.ProductID
    WHERE sl.ShelfID IS NULL;';
GO

IF EXISTS (SELECT 1 FROM dbo.SaleLine WHERE ShelfID IS NULL)
BEGIN
    THROW 50010, 'Could not assign a shelf to every existing sale line.', 1;
END;
GO

IF COLUMNPROPERTY(OBJECT_ID('dbo.SaleLine'), 'ShelfID', 'AllowsNull') = 1
BEGIN
    ALTER TABLE dbo.SaleLine ALTER COLUMN ShelfID INT NOT NULL;
END;
GO

IF COL_LENGTH('dbo.SaleLine', 'Comment') IS NULL
BEGIN
    ALTER TABLE dbo.SaleLine ADD Comment NVARCHAR(100) NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_SaleLine_Shelf'
      AND parent_object_id = OBJECT_ID('dbo.SaleLine')
)
BEGIN
    ALTER TABLE dbo.SaleLine
    ADD CONSTRAINT FK_SaleLine_Shelf
        FOREIGN KEY (ShelfID) REFERENCES dbo.Shelf(ShelfID);
END;
GO

IF COL_LENGTH('dbo.Settlement', 'TotalReturns') IS NULL
BEGIN
    ALTER TABLE dbo.Settlement
    ADD TotalReturns DECIMAL(10,2) NOT NULL
        CONSTRAINT DF_Settlement_TotalReturns DEFAULT (0);
END;
GO

IF COL_LENGTH('dbo.Settlement', 'LastModifiedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Settlement ADD LastModifiedAt DATETIME2(0) NULL;
END;
GO

IF COL_LENGTH('dbo.Settlement', 'ModifiedByEmployeeID') IS NULL
BEGIN
    ALTER TABLE dbo.Settlement ADD ModifiedByEmployeeID INT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_Settlement_ModifiedByEmployee'
      AND parent_object_id = OBJECT_ID('dbo.Settlement')
)
BEGIN
    ALTER TABLE dbo.Settlement
    ADD CONSTRAINT FK_Settlement_ModifiedByEmployee
        FOREIGN KEY (ModifiedByEmployeeID) REFERENCES dbo.Employee(EmployeeID);
END;
GO

-- Seed the 80 fixed shelves only when the database has no shelves yet.
IF NOT EXISTS (SELECT 1 FROM dbo.Shelf)
BEGIN
    ;WITH ShelfNumbers AS
    (
        SELECT 1 AS Number
        UNION ALL
        SELECT Number + 1 FROM ShelfNumbers WHERE Number < 80
    )
    INSERT INTO dbo.Shelf
        (Number, ShelfStatus, Location, NumberOfShelves, NumberOfClothingRails)
    SELECT Number, N'Available', NULL, 6, 0
    FROM ShelfNumbers
    OPTION (MAXRECURSION 80);
END;
GO
