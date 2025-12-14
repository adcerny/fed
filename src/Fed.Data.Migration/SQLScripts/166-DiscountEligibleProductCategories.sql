SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DiscountEligibleProductCategories](
	[DiscountId] [UNIQUEIDENTIFIER] NOT NULL,
	[ProductCategoryId] [UNIQUEIDENTIFIER] NOT NULL,
 CONSTRAINT [PK_DiscountEligibleProductCategories] PRIMARY KEY CLUSTERED 
(
	[DiscountId] ASC,
	[ProductCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DiscountEligibleProductCategories]  WITH CHECK ADD  CONSTRAINT [FK_DiscountEligibleProductCategories_Discounts] FOREIGN KEY([DiscountId])
REFERENCES [dbo].[Discounts] ([Id])
GO

ALTER TABLE [dbo].[DiscountEligibleProductCategories] CHECK CONSTRAINT [FK_DiscountEligibleProductCategories_Discounts]
GO

ALTER TABLE [dbo].[DiscountEligibleProductCategories]  WITH CHECK ADD  CONSTRAINT [FK_DiscountEligibleProductCategories_ProductCategories] FOREIGN KEY([ProductCategoryId])
REFERENCES [dbo].[ProductCategories] ([Id])
GO

ALTER TABLE [dbo].[DiscountEligibleProductCategories] CHECK CONSTRAINT [FK_DiscountEligibleProductCategories_ProductCategories]
GO