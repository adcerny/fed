SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'IsFriend')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [IsFriend] BIT NOT NULL DEFAULT 0
END