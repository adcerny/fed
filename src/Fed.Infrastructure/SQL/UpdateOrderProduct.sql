UPDATE [dbo].[OrderProducts]
   SET [Quantity] = @Quantity,
       [ProductCode] = @ProductCode,
       [ProductGroup] = @ProductGroup,
       [ProductName] = @ProductName,
       [SupplierId] = @SupplierId,
       [SupplierSKU] = @SupplierSKU,
       [Price] = @Price,
       [SalePrice] = @SalePrice,
       [IsTaxable] = @IsTaxable,
       [RefundablePrice] = @RefundablePrice
 WHERE [OrderId] = @OrderId AND
       [ProductId] = @ProductId
       