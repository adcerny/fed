SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Products]') AND [name] = 'IconCategory')
BEGIN
    ALTER TABLE [dbo].[Products] ADD [IconCategory] NVARCHAR(250) NOT NULL
	CONSTRAINT DV_Products_IconCategory DEFAULT ('') WITH VALUES
END