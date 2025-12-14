SELECT
	[da].*,
	[p].[ProductName]
FROM [dbo].[DeliveryAdditions] AS [da]
LEFT JOIN [dbo].[Products] AS [p] ON [da].[ProductId] = [p].[Id]
WHERE [da].[OrderId] = @OrderId AND [da].[ProductId] = @ProductId