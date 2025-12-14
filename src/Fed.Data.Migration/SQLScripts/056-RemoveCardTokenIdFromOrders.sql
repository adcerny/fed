SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
IF EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Orders]') AND [name] = N'CardTokenId')
	ALTER TABLE [dbo].[Orders] DROP COLUMN [CardTokenId]