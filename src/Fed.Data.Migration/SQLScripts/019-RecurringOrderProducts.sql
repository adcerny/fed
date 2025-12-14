SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RecurringOrderProducts](
	[RecurringOrderId] [uniqueidentifier] NOT NULL,
	[ProductId] [nvarchar](200) NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_RecurringOrderProducts] PRIMARY KEY CLUSTERED 
(
	[RecurringOrderId] ASC,
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RecurringOrderProducts]  WITH CHECK ADD  CONSTRAINT [FK_RecurringOrderProducts_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO

ALTER TABLE [dbo].[RecurringOrderProducts] CHECK CONSTRAINT [FK_RecurringOrderProducts_Products]
GO

ALTER TABLE [dbo].[RecurringOrderProducts]  WITH CHECK ADD  CONSTRAINT [FK_RecurringOrderProducts_RecurringOrder] FOREIGN KEY([RecurringOrderId])
REFERENCES [dbo].[RecurringOrders] ([Id])
GO

ALTER TABLE [dbo].[RecurringOrderProducts] CHECK CONSTRAINT [FK_RecurringOrderProducts_RecurringOrder]
GO