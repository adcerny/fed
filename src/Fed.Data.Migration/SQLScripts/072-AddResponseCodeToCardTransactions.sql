IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[CardTransactions]') AND [name] = 'ResponseText')
BEGIN
    ALTER TABLE [dbo].[CardTransactions] ADD [ResponseText] NVARCHAR(250) NULL
END
GO

IF NOT EXISTS (SELECT [Id] FROM [dbo].[CardTransactions] WHERE [ResponseText] IS NOT NULL)
BEGIN
	UPDATE [dbo].[CardTransactions] SET [ResponseText] = [ResponseCode], [ResponseCode] = NULL
END
