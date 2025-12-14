IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Deliveries]') AND [name] = 'DeliveryCharge')
BEGIN
    ALTER TABLE [dbo].[Deliveries] ADD [DeliveryCharge] [decimal](18, 2) NOT NULL DEFAULT 0
END

