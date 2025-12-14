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
SELECT 
    @OrderId,
    [p].[Id],
    @Quantity,
    @ProductCode,
    [p].[ProductGroup],
    [p].[ProductName],
    [p].[SupplierId],
    [p].[SupplierSKU],
    [p].[Price],
    @SalePrice,
	@RefundablePrice,
    [p].[IsTaxable]

    FROM [dbo].[Products] AS [p] WHERE [p].[ProductCode] = @ProductCode AND [p].[IsDeleted] = 0