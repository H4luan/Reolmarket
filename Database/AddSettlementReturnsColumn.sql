USE ShelfMarket;
GO

IF COL_LENGTH('dbo.Settlement', 'TotalReturns') IS NULL
BEGIN
    ALTER TABLE dbo.Settlement
    ADD TotalReturns DECIMAL(10,2) NOT NULL
        CONSTRAINT DF_Settlement_TotalReturns DEFAULT (0);
END;
GO