CREATE TABLE [dbo].[InvoiceDeliveries](
	[InvoiceId] [UNIQUEIDENTIFIER] NOT NULL,
	[DeliveryId] [UNIQUEIDENTIFIER] NOT NULL,
 CONSTRAINT [PK_InvoiceDeliveries] PRIMARY KEY CLUSTERED 
(
	[InvoiceId] ASC,
	[DeliveryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[InvoiceDeliveries]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceDeliveries_Invoices] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[Invoices] ([Id])
GO

ALTER TABLE [dbo].[InvoiceDeliveries] CHECK CONSTRAINT [FK_InvoiceDeliveries_Invoices]
GO

ALTER TABLE [dbo].[InvoiceDeliveries]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceDeliveries_Deliveries] FOREIGN KEY([DeliveryId])
REFERENCES [dbo].[Deliveries] ([Id])
GO

ALTER TABLE [dbo].[InvoiceDeliveries] CHECK CONSTRAINT [FK_InvoiceDeliveries_Deliveries]
GO