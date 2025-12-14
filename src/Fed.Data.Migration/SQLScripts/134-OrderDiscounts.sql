SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[OrderDiscounts](
	[OrderId] [UNIQUEIDENTIFIER] NOT NULL,
	[DiscountId] [UNIQUEIDENTIFIER] NOT NULL,
	[Name] [NVARCHAR](250) NOT NULL,
	[Description] [NVARCHAR](MAX) NOT NULL,
	[Percentage] [DECIMAL](8, 2) NULL,
	[Value] [DECIMAL](18, 2) NULL,
	[OrderTotalDeduction] [DECIMAL](18, 2) NOT NULL,
 CONSTRAINT [PK_OrderDiscounts] PRIMARY KEY CLUSTERED 
(
	[DiscountId] ASC,
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


ALTER TABLE [dbo].[OrderDiscounts]  WITH CHECK ADD  CONSTRAINT [FK_OrderDiscounts_Orders] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
GO

ALTER TABLE [dbo].[OrderDiscounts] CHECK CONSTRAINT [FK_OrderDiscounts_Orders]
GO

ALTER TABLE [dbo].[OrderDiscounts]  WITH CHECK ADD  CONSTRAINT [FK_OrderDiscounts_Discounts] FOREIGN KEY([DiscountId])
REFERENCES [dbo].[Discounts] ([Id])
GO

ALTER TABLE [dbo].[OrderDiscounts] CHECK CONSTRAINT [FK_OrderDiscounts_Discounts]
GO

