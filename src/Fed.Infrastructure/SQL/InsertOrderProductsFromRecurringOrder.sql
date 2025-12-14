INSERT INTO [dbo].[OrderProducts]
(
    [OrderId],
    [ProductId],
    [Quantity],
    [ProductCode],
    [ProductGroup],
    [ProductName],
    [SupplierId],
    [SupplierSKU],
    [Price],
    [SalePrice],
	[RefundablePrice],
    [IsTaxable]
)
SELECT @OrderId,
       [pr].[Id],
       [rp].[Quantity],
       [pr].[ProductCode],
       [pr].[ProductGroup],
       [pr].[ProductName],
       [pr].[SupplierId],
       [pr].[SupplierSKU],
       CASE 
			WHEN @IsFree = 1 THEN 0 
			ELSE [pr].[Price] 
	   END,
       CASE 
			WHEN @IsFree = 1 AND [pr].[SalePrice] IS NOT NULL THEN 0 
			ELSE [pr].[SalePrice] 
	   END,
	   CASE 
			WHEN @IsFree = 1 THEN 0 
			ELSE [pr].[Price] 
	   END,
       [pr].[IsTaxable]
FROM [dbo].[RecurringOrderProducts] AS [rp],
     [dbo].[Products] AS [pr]
WHERE [rp].[RecurringOrderId] = @RecurringOrderId
      AND [pr].[Id] = [rp].[ProductId];