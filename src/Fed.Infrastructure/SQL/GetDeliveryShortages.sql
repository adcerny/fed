SELECT
	[ds].*,
	[p].[ProductName]
FROM [dbo].[DeliveryShortages] AS [ds]
LEFT JOIN [dbo].[Products] AS [p] ON [ds].[ProductId] = [p].[Id]
INNER JOIN [dbo].[Orders] AS [o] ON [o].[Id] = [ds].[OrderId]
	WHERE [o].[DeliveryDate] >= @FromDate 
	AND [o].[DeliveryDate] <= @ToDate