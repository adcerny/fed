SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[RecurringOrders]') AND [name] = 'IsFree')
BEGIN
    ALTER TABLE [dbo].[RecurringOrders] ADD [IsFree] BIT NOT NULL DEFAULT 0
END

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Orders]') AND [name] = 'IsFree')
BEGIN
    ALTER TABLE [dbo].[Orders] ADD [IsFree] BIT NOT NULL DEFAULT 0
END