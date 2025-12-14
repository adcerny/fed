INSERT INTO [dbo].[DiscountQualifyingProductCategories]
(
	[DiscountId],
	[Quantity],
	[ProductCategoryId]
)
VALUES
(	
	@DiscountId,
	@Quantity,
	@ProductCategoryId
)