SELECT
	[o].[DeliveryDate],
	[o].[EarliestTime],
	[o].[LatestTime],
	[o].[CompanyName],
	[o].[ShortId] AS [OrderId],
	[o].[OrderName],
	[op].[ProductGroup],
	[op].[ProductCode],
	[op].[ProductName],
	[op].[Quantity]
FROM [dbo].[Deliveries] AS [d]
INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [d].[Id] = [do].[DeliveryId]
INNER JOIN [dbo].[Orders] AS [o] ON [do].[OrderId] = [o].[Id]
INNER JOIN [dbo].[OrderProducts] AS [op] ON [o].[Id] = [op].[OrderId]
WHERE [d].[ShortId] = @DeliveryShortId