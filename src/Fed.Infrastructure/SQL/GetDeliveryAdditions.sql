SELECT
	[da].*,
	[p].[ProductName],
	[p].[ProductCode],
	ISNULL([p].[SalePrice], [p].[Price]) AS [ProductPrice],
	[p].[IsTaxable]
FROM [dbo].[DeliveryAdditions] AS [da]
LEFT JOIN [dbo].[Products] AS [p] ON [da].[ProductId] = [p].[Id]
INNER JOIN [dbo].[Orders] AS [o] ON [o].[Id] = [da].[OrderId]
	WHERE [o].[DeliveryDate] >= @FromDate 
	AND [o].[DeliveryDate] <= @ToDate