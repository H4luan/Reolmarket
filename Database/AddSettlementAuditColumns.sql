USE ShelfMarket;
GO

IF COL_LENGTH('dbo.Settlement', 'LastModifiedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Settlement
        ADD LastModifiedAt DATETIME2(0) NULL;
END;
GO

IF COL_LENGTH('dbo.Settlement', 'ModifiedByEmployeeID') IS NULL
BEGIN
    ALTER TABLE dbo.Settlement
        ADD ModifiedByEmployeeID INT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Settlement_ModifiedByEmployee'
      AND parent_object_id = OBJECT_ID('dbo.Settlement')
)
BEGIN
    ALTER TABLE dbo.Settlement
        ADD CONSTRAINT FK_Settlement_ModifiedByEmployee
        FOREIGN KEY (ModifiedByEmployeeID)
        REFERENCES dbo.Employee(EmployeeID);
END;
GO
