/****** Object:  Table [dbo].[DiscountProducts]    Script Date: 18/12/2019 13:49:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DiscountQualifyingProducts](
	[ProductId] [NVARCHAR](200) NOT NULL,
	[DiscountId] [UNIQUEIDENTIFIER] NOT NULL,
	[Quantity] [INT] NOT NULL,
 CONSTRAINT [PK_DiscountQualifyingProducts] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC,
	[DiscountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DiscountQualifyingProducts]  WITH CHECK ADD  CONSTRAINT [FK_DiscountQualifyingProducts_Discounts] FOREIGN KEY([DiscountId])
REFERENCES [dbo].[Discounts] ([Id])
GO

ALTER TABLE [dbo].[DiscountQualifyingProducts] CHECK CONSTRAINT [FK_DiscountQualifyingProducts_Discounts]
GO

ALTER TABLE [dbo].[DiscountQualifyingProducts]  WITH CHECK ADD  CONSTRAINT [FK_DiscountQualifyingProducts_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO

ALTER TABLE [dbo].[DiscountQualifyingProducts] CHECK CONSTRAINT [FK_DiscountQualifyingProducts_Products]
GO