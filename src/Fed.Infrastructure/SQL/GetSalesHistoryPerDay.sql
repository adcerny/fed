SELECT
	[od].[DeliveryDate] AS [Date],
	DATENAME(dw, [od].[DeliveryDate]) AS [Day],
	COUNT(DISTINCT [od].[ContactId]) AS [Customers],
	COUNT(DISTINCT [od].[Id]) AS [TotalOrderCount],
	COUNT(DISTINCT [do].[DeliveryId]) AS [TotalDeliveryCount],
	SUM(ISNULL([op].[SalePrice], [op].[Price]) * [op].[Quantity]) AS [TotalOrderValue]
FROM [dbo].[Orders] AS [od]
INNER JOIN [dbo].[OrderProducts] AS [op] ON [od].[Id] = [op].[OrderId]
INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [od].[Id] = [do].[OrderId]
INNER JOIN [dbo].[Customers] AS [c] ON [c].[Id] = [od].[CustomerId] AND [c].[AccountTypeId] <> 1 --Exclude internal
WHERE
	[od].[DeliveryDate] >= @FromDate
	AND [od].[DeliveryDate] <= @ToDate
GROUP BY
	[od].[DeliveryDate]