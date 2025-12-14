IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'SplitDeliveriesByOrder')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [SplitDeliveriesByOrder] [bit] NOT NULL DEFAULT 0
END

GO