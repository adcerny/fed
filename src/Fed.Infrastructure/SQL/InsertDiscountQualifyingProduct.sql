DECLARE @ProductId UNIQUEIDENTIFIER
SELECT @ProductId = Id FROM Products WHERE ProductCode = @ProductCode

INSERT INTO [dbo].[DiscountQualifyingProducts]
(
	[ProductId],
	[DiscountId],
	[Quantity]
)
VALUES
(	
	@ProductId,
	@DiscountId,
	@Quantity
)