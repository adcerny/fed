IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'IsInvoiceable')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [IsInvoiceable] BIT NOT NULL
	CONSTRAINT DV_Customers_IsInvoiceable DEFAULT (1) WITH VALUES
END

