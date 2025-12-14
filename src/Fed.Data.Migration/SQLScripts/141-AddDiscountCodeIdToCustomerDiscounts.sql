ALTER TABLE [dbo].[CustomerDiscounts] ADD [DiscountCodeId] UNIQUEIDENTIFIER NULL,
 FOREIGN KEY(DiscountCodeId) REFERENCES DiscountCodes(Id);