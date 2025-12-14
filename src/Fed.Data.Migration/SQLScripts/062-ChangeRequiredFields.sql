SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[Contacts] ALTER COLUMN [Title] [nvarchar](250) NULL

UPDATE [dbo].[Contacts]
SET [Title] = NULL
WHERE [Id] IN (SELECT [Id] FROM [dbo].[Contacts] WHERE TRIM([Title]) = '')

UPDATE [dbo].[Contacts]
SET [Title] = NULL
WHERE [Id] IN (SELECT [Id] FROM [dbo].[Contacts] WHERE TRIM([Title]) = '.')