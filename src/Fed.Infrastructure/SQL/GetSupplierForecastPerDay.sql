SELECT
	[prd].[SupplierId],
	[prd].[SupplierSKU],
	[prd].[ProductCode],
	[prd].[ProductName],
	SUM([rop].[Quantity]) AS [FedQuantity],
	CEILING(
		SUM(
			CAST(([rop].[Quantity] * ISNULL([uc].[FedUnits], 1)) AS float) /
			CAST((ISNULL([uc].[SupplierUnits], 1)) AS float)
	)) AS [SupplierQuantity],
	COUNT([rop].[ProductId]) AS [CustomerCount],
	STRING_AGG([cu].[CompanyName], ', ') AS [Customers]

FROM [dbo].[Products] AS [prd]

LEFT JOIN [dbo].[RecurringOrderProducts] AS [rop] ON [prd].[Id] = [rop].[ProductId]
INNER JOIN [dbo].[RecurringOrders] AS [ro] ON [ro].[Id] = [rop].[RecurringOrderId]
INNER JOIN [dbo].[Timeslots] AS [ts] ON [ro].[TimeslotId] = [ts].[Id]	
LEFT JOIN [dbo].[Holidays] AS [hd] ON [hd].[Date] = @Date
LEFT JOIN [dbo].[SkipDates] AS [sd] ON [ro].[Id] = [sd].[RecurringOrderId] AND [sd].[Date] = @Date
LEFT JOIN [dbo].[ProductUnitConversions] AS [uc] ON [prd].[SupplierId] = [uc].[SupplierId] AND [prd].[SupplierSKU] = [uc].[SupplierSKU]
INNER JOIN [dbo].[Contacts] AS [co] ON [co].[Id] = [ro].[ContactId]
INNER JOIN [dbo].[Customers] AS [cu] ON [cu].[Id] = [co].[CustomerId]
INNER JOIN [dbo].[AccountTypes] AS [at] ON [cu].[AccountTypeId] = [at].[Id]

WHERE
	[ro].[IsDeleted] = 0											-- Not deleted
	AND [ro].[StartDate] <= @Date									-- Past the start date
	AND [ro].[EndDate] >= @Date										-- Before the end date
	AND [hd].[Date] IS NULL											-- No bank holiday
	AND [ro].[EndDate] >= [ro].[StartDate]							-- Not deleted in old way
	AND [sd].[Date] IS NULL											-- Not skipped
	AND [ts].[DayOfWeek]  = (DATEPART(dw, @Date) - 1)				-- Correct day of week
	AND (@SupplierId IS NULL OR [prd].[SupplierId] = @SupplierId)	-- Filter by supplier if provided
	AND [at].[AccountType] <> 'Deleted'
	AND [at].[AccountType] <> 'Cancelled'
	AND [at].[AccountType] <> 'Demo'
	AND [at].[AccountType] <> 'Paused'

GROUP BY
	[prd].[SupplierSKU],
	[prd].[SupplierId],
	[prd].[ProductCode],
	[prd].[ProductName]

ORDER BY
	[prd].[SupplierId],
	[prd].[SupplierSKU]