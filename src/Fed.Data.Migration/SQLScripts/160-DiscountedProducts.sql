/****** Object:  Table [dbo].[DiscountedProducts]    Script Date: 18/12/2019 13:49:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DiscountedProducts](
	[ProductId] [NVARCHAR](200) NOT NULL,
	[DiscountId] [UNIQUEIDENTIFIER] NOT NULL,
	[Price] [DECIMAL](18, 2) NULL,
	[Quantity] [INT] NOT NULL,
 CONSTRAINT [PK_DiscountedProducts] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC,
	[DiscountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DiscountedProducts]  WITH CHECK ADD  CONSTRAINT [FK_DiscountedProducts_Discounts] FOREIGN KEY([DiscountId])
REFERENCES [dbo].[Discounts] ([Id])
GO

ALTER TABLE [dbo].[DiscountedProducts] CHECK CONSTRAINT [FK_DiscountedProducts_Discounts]
GO

ALTER TABLE [dbo].[DiscountedProducts]  WITH CHECK ADD  CONSTRAINT [FK_DiscountedProducts_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO

ALTER TABLE [dbo].[DiscountedProducts] CHECK CONSTRAINT [FK_DiscountedProducts_Products]
GO