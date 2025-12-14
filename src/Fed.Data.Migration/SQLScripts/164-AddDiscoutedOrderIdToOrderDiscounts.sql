ALTER TABLE [dbo].[OrderDiscounts] ADD [DiscountedProductsOrderId] UNIQUEIDENTIFIER NULL

ALTER TABLE [dbo].[OrderDiscounts]  WITH CHECK ADD  CONSTRAINT [FK_OrderDiscounts_DiscountedProductsOrderId] FOREIGN KEY([DiscountedProductsOrderId])
REFERENCES [dbo].[Orders] ([Id])
