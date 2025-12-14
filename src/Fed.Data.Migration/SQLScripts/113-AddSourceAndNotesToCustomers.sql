SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'Source')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [Source] NVARCHAR(255) NULL
END

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'Notes')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [Notes] NVARCHAR(MAX) NULL
END