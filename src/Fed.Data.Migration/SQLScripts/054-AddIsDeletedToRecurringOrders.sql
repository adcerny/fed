/****** Object:  Table [dbo].[ProductUnitConversions]    Script Date: 14/02/2019 13:14:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[RecurringOrders]') AND [name] = N'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[RecurringOrders] ADD [IsDeleted] [bit] NOT NULL
	CONSTRAINT DV_RecurringOrders_IsDeleted DEFAULT (0) WITH VALUES
END

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[RecurringOrders]') AND [name] = N'DeletedDate')
BEGIN
    ALTER TABLE [dbo].[RecurringOrders] ADD [DeletedDate] [datetime2](7) NULL
END