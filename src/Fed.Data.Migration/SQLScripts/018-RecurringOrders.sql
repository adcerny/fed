SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RecurringOrders](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[ContactId] [uniqueidentifier] NOT NULL,
	[DeliveryAddressId] [uniqueidentifier] NOT NULL,
	[BillingAddressId] [uniqueidentifier] NOT NULL,
	[CardTokenId] [uniqueidentifier] NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[WeeklyRecurrence] [int] NOT NULL,
	[TimeslotId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_RecurringOrders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RecurringOrders]  WITH CHECK ADD  CONSTRAINT [FK_RecurringOrders_Timeslots] FOREIGN KEY([TimeslotId])
REFERENCES [dbo].[Timeslots] ([Id])
GO

ALTER TABLE [dbo].[RecurringOrders] CHECK CONSTRAINT [FK_RecurringOrders_Timeslots]
GO

ALTER TABLE [dbo].[RecurringOrders]  WITH CHECK ADD  CONSTRAINT [FK_RecurringOrders_Contacts] FOREIGN KEY([ContactId])
REFERENCES [dbo].[Contacts] ([Id])
GO

ALTER TABLE [dbo].[RecurringOrders] CHECK CONSTRAINT [FK_RecurringOrders_Contacts]
GO

ALTER TABLE [dbo].[RecurringOrders]  WITH CHECK ADD  CONSTRAINT [FK_RecurringOrders_DeliveryAddresses] FOREIGN KEY([DeliveryAddressId])
REFERENCES [dbo].[DeliveryAddresses] ([Id])
GO

ALTER TABLE [dbo].[RecurringOrders] CHECK CONSTRAINT [FK_RecurringOrders_DeliveryAddresses]
GO

ALTER TABLE [dbo].[RecurringOrders]  WITH CHECK ADD  CONSTRAINT [FK_RecurringOrders_BillingAddresses] FOREIGN KEY([BillingAddressId])
REFERENCES [dbo].[BillingAddresses] ([Id])
GO

ALTER TABLE [dbo].[RecurringOrders] CHECK CONSTRAINT [FK_RecurringOrders_BillingAddresses]
GO