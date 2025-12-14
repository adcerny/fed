DECLARE @ProductId UNIQUEIDENTIFIER
SELECT @ProductId = Id FROM [dbo].[Products] WHERE ProductCode = @ProductCode


INSERT INTO [dbo].[DiscountedProducts]
(
    [ProductId],
    [DiscountId],
    [Price],
    [Quantity]
)

VALUES
(   @ProductId,
    @DiscountId,
    @Price,
    @Quantity
)