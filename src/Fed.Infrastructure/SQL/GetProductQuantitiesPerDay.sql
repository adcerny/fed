SELECT
	[p].[ProductGroup],
	[op].[ProductCode],
	[p].[SupplierId],
	ISNULL([s].[Name], 'Unkown')  AS [Supplier],
	COUNT([op].[ProductCode]) AS 'Total Orders',
	SUM([op].[Quantity]) AS 'Total Quantity',
	[o].[DeliveryDate]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[OrderProducts] AS [op] ON [o].[Id] = [op].[OrderId]
LEFT JOIN [dbo].[Products] AS [p] ON [p].[Id] = [op].[ProductId] AND [p].[ProductGroup] <> '<Missing in Merchello>'
LEFT JOIN [dbo].[Suppliers] AS [s] ON [s].[Id] =[p].[SupplierId]
GROUP BY
	[p].[ProductGroup],
	[op].[ProductCode],
	[p].[SupplierId],
	[o].[DeliveryDate],
	[s].[Name]
ORDER BY [o].[DeliveryDate] DESC