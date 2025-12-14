SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'OfficeSizeMin')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [OfficeSizeMin] INT NULL
END

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'OfficeSizeMax')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [OfficeSizeMax] INT NULL
END