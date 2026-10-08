USE ShelfMarket;
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

-- Dynamic SQL runs after the new column exists as a separate batch.
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

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SaleLine_Shelf'
)
BEGIN
    ALTER TABLE dbo.SaleLine
    ADD CONSTRAINT FK_SaleLine_Shelf
        FOREIGN KEY (ShelfID) REFERENCES dbo.Shelf(ShelfID);
END;
GO

IF COL_LENGTH('dbo.SaleLine', 'Comment') IS NULL
BEGIN
    ALTER TABLE dbo.SaleLine ADD Comment NVARCHAR(100) NULL;
END;
GO