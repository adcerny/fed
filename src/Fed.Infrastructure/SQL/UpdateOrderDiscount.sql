UPDATE [dbo].[OrderDiscounts]
SET [DiscountedProductsOrderId] = @DiscountedProductsOrderId
WHERE [OrderId] = @OrderId
      AND [DiscountId] = @DiscountId;