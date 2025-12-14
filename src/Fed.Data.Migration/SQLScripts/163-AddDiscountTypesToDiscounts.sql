ALTER TABLE [dbo].[Discounts]
ADD [DiscountRewardTypeId] INT NOT NULL DEFAULT 1,
[DiscountEligibleProductsTypeId] INT NOT NULL DEFAULT 1,
[DiscountQualificationTypeId] INT NOT NULL DEFAULT 1

ALTER TABLE [dbo].[Discounts]  WITH CHECK ADD  CONSTRAINT [FK_Discounts_DiscountRewardType] FOREIGN KEY([DiscountRewardTypeId])
REFERENCES [dbo].[DiscountRewardType] ([Id])
GO

ALTER TABLE [dbo].[Discounts]  WITH CHECK ADD  CONSTRAINT [FK_Discounts_DiscountEligibleProductsType] FOREIGN KEY([DiscountEligibleProductsTypeId])
REFERENCES [dbo].[DiscountEligibleProductsType] ([Id])
GO

ALTER TABLE [dbo].[Discounts]  WITH CHECK ADD  CONSTRAINT [FK_Discounts_DiscountQualificationType] FOREIGN KEY([DiscountQualificationTypeId])
REFERENCES [dbo].[DiscountQualificationType] ([Id])
GO