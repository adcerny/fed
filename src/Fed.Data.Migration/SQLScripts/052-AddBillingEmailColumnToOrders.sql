SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Orders]') AND [name] = 'BillingEmail')
BEGIN
    ALTER TABLE Orders ADD BillingEmail VARCHAR(250) NULL
END