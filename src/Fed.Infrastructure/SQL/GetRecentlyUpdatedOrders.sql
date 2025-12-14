SELECT
	DISTINCT *
FROM [dbo].[RecurringOrders] AS [RecurringOrders]
INNER JOIN (
	SELECT
		[ro].[Id]
	FROM [dbo].[RecurringOrders] AS [ro]
	INNER JOIN [dbo].[RecurringOrderProducts] AS [rop] ON [ro].[Id] = [rop].[RecurringOrderId]
	WHERE
		[ro].[LastUpdatedDate] >= DATEADD(DD, -5, GETDATE())
	GROUP BY
		[ro].[Id]) AS [filtered1] ON [RecurringOrders].[Id] = [filtered1].[Id]
INNER JOIN (
	SELECT
		[ro].[Id]
	FROM [dbo].[RecurringOrders] AS [ro]
	INNER JOIN [dbo].[RecurringOrderProducts] AS [rop] ON [ro].[Id] = [rop].[RecurringOrderId]
	WHERE
		[rop].[AddedDate] >= DATEADD(DD, -5, GETDATE())
	GROUP BY
		[ro].[Id]) AS [filtered2] ON [RecurringOrders].[Id] = [filtered2].[Id]
ORDER BY
	[RecurringOrders].[LastUpdatedDate]