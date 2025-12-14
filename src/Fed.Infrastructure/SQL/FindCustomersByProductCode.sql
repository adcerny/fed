SELECT
	[p].[ProductCode],
	[cu].[CompanyName],
	[co].[Email],
	COUNT([rod].[Id]) AS [Orders],
	STRING_AGG('[' + (CASE [ts].[DayOfWeek]
		WHEN 1 THEN 'Mon'
		WHEN 2 THEN 'Tue'
		WHEN 3 THEN 'Wed'
		WHEN 4 THEN 'Thu'
		WHEN 5 THEN 'Fri'
	  END) + ' ' + CAST((DATEPART(hour, [ts].[EarliestTime])) AS varchar(10)) + '-' + CAST((DATEPART(hour, [ts].[LatestTime])) AS varchar(10)) + ']', ', ') AS [Timeslots],
	SUM([rop].[Quantity]) AS [Total Quantity]
FROM [dbo].[Products] AS [p]
INNER JOIN [dbo].[RecurringOrderProducts] AS [rop] ON [p].[Id] = [rop].[ProductId]
INNER JOIN [dbo].[RecurringOrders] AS [rod] ON [rod].[Id] = [rop].[RecurringOrderId]
INNER JOIN [dbo].[Contacts] AS [co] ON [rod].[ContactId] = [co].[Id]
INNER JOIN [dbo].[Customers] AS [cu] ON [co].[CustomerId] = [cu].[Id]
INNER JOIN [dbo].[Timeslots] AS [ts] ON [rod].[TimeslotId] = [ts].[Id]
WHERE 
	([p].[ProductName] LIKE '%' + @ProductCode + '%'
	OR [p].[ProductCode] LIKE '%' + @ProductCode + '%')
	AND [rod].[IsDeleted] = 0
	AND [rod].[EndDate] > GETDATE()
	AND [rod].[StartDate] <= [rod].[EndDate]
GROUP BY
	[p].[ProductCode],
	[cu].[CompanyName],
	[co].[Email]
ORDER BY
	[cu].[CompanyName]