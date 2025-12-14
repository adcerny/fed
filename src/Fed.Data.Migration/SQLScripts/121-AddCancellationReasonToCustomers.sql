SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'CancellationReason')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [CancellationReason] NVARCHAR(MAX) NULL
END