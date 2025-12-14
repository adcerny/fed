IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Timeslots]') AND [name] = 'DeliveryCharge')
BEGIN
    ALTER TABLE [dbo].[Timeslots] ADD [DeliveryCharge] [decimal](18, 2) NOT NULL DEFAULT 0
END

GO

UPDATE [dbo].[Timeslots] SET DeliveryCharge = 3