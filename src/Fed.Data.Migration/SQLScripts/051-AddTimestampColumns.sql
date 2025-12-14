/****** Object:  Table [dbo].[ProductUnitConversions]    Script Date: 14/02/2019 13:14:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[Hubs] ADD [CreatedDate] [datetime2](7) NOT NULL
CONSTRAINT DV_Hubs_CreatedDate DEFAULT (GETDATE()) WITH VALUES

ALTER TABLE [dbo].[Customers] ADD [RegisterDate] [datetime2](7) NOT NULL
CONSTRAINT DV_Customers_RegisterDate DEFAULT (GETDATE()) WITH VALUES

ALTER TABLE [dbo].[Contacts] ADD [CreatedDate] [datetime2](7) NOT NULL
CONSTRAINT DV_Contacts_CreatedDate DEFAULT (GETDATE()) WITH VALUES

ALTER TABLE [dbo].[RecurringOrders] ADD [CreatedDate] [datetime2](7) NOT NULL
CONSTRAINT DV_RecurringOrders_CreatedDate DEFAULT (GETDATE()) WITH VALUES

ALTER TABLE [dbo].[RecurringOrderProducts] ADD [AddedDate] [datetime2](7) NOT NULL
CONSTRAINT DV_RecurringOrderProducts_AddedDate DEFAULT (GETDATE()) WITH VALUES

ALTER TABLE [dbo].[CardTokens] ADD [CreatedDate] [datetime2](7) NOT NULL
CONSTRAINT DV_CardTokens_CreatedDate DEFAULT (GETDATE()) WITH VALUES