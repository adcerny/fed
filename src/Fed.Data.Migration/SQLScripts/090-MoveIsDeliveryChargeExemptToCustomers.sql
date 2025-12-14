SET QUOTED_IDENTIFIER ON;
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'IsDeliveryChargeExempt')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [IsDeliveryChargeExempt] [bit] NOT NULL DEFAULT 0
END

GO

UPDATE [dbo].[Customers] SET [IsDeliveryChargeExempt] = 1;

GO


IF EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Contacts]') AND [name] = 'IsDeliveryChargeExempt')
BEGIN	
	ALTER TABLE [dbo].[Contacts] DROP COLUMN [IsDeliveryChargeExempt]
END

COMMIT;
GO
