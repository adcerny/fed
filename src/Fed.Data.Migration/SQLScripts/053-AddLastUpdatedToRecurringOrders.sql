SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[RecurringOrders]') AND [name] = N'LastUpdatedDate')
BEGIN
    ALTER TABLE [dbo].[RecurringOrders] ADD [LastUpdatedDate] [datetime2](7) NOT NULL
	CONSTRAINT DV_RecurringOrders_LastUpdatedDate DEFAULT (GETDATE()) WITH VALUES
END