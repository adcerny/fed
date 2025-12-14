DECLARE @ThisWeek INT
SET @ThisWeek = DATEPART(WEEK, GETDATE())

SELECT
	[cu].[AccountTypeId],
	[at].[AccountType],
	DATEPART(ISO_WEEK, [dl].[DeliveryDate]) AS [Week],
	SUM((ISNULL([op].[SalePrice], [op].[Price]) * [op].[Quantity])) AS [Sales],
	COUNT(DISTINCT [dl].[Id]) AS [Deliveries]
FROM [dbo].[Orders] AS [od]
INNER JOIN [dbo].[OrderProducts] AS [op] ON [od].[Id] = [op].[OrderId]
INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [od].[Id] = [do].[OrderId]
INNER JOIN [dbo].[Deliveries] AS [dl] ON [do].[DeliveryId] = [dl].[Id]
INNER JOIN [dbo].[Customers] AS [cu] ON [od].[CustomerId] = [cu].[Id]
INNER JOIN [dbo].[AccountTypes] AS [at] ON [cu].[AccountTypeId] = [at].[Id]
WHERE
	DATEPART(ISO_WEEK, [dl].[DeliveryDate]) >= @ThisWeek - 3
GROUP BY
	[cu].[AccountTypeId],
	[at].[AccountType],
	DATEPART(ISO_WEEK, [dl].[DeliveryDate])
ORDER BY
	[cu].[AccountTypeId] ASC,
	[Week] DESC