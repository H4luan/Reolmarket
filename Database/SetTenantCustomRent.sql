USE ShelfMarket;
GO

IF COL_LENGTH('dbo.ShelfRenter', 'UseCustomRent') IS NULL
BEGIN
    ALTER TABLE dbo.ShelfRenter
    ADD UseCustomRent BIT NOT NULL
        CONSTRAINT DF_ShelfRenter_UseCustomRent DEFAULT (0);
END;
GO

IF COL_LENGTH('dbo.ShelfRenter', 'CustomRentPerShelf') IS NULL
BEGIN
    ALTER TABLE dbo.ShelfRenter
    ADD CustomRentPerShelf DECIMAL(10, 2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_ShelfRenter_CustomRent'
      AND parent_object_id = OBJECT_ID('dbo.ShelfRenter')
)
BEGIN
    ALTER TABLE dbo.ShelfRenter
    ADD CONSTRAINT CK_ShelfRenter_CustomRent
        CHECK (
            (UseCustomRent = 0 AND CustomRentPerShelf IS NULL)
            OR (UseCustomRent = 1 AND CustomRentPerShelf > 0)
        );
END;
GO
