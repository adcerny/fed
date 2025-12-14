SELECT
	[ds].*,
	[p].[ProductName]
FROM [dbo].[DeliveryShortages] AS [ds]
LEFT JOIN [dbo].[Products] AS [p] ON [ds].[ProductId] = [p].[Id]
WHERE [ds].[OrderId] = @OrderId AND [ds].[ProductId] = @ProductId