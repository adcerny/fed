SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[InvoiceOrders](
	[InvoiceId] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_InvoiceOrders] PRIMARY KEY CLUSTERED 
(
	[InvoiceId] ASC,
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[InvoiceOrders]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceOrders_Invoices] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[Invoices] ([Id])
GO

ALTER TABLE [dbo].[InvoiceOrders] CHECK CONSTRAINT [FK_InvoiceOrders_Invoices]
GO

ALTER TABLE [dbo].[InvoiceOrders]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceOrders_Orders] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
GO

ALTER TABLE [dbo].[InvoiceOrders] CHECK CONSTRAINT [FK_InvoiceOrders_Orders]
GO

