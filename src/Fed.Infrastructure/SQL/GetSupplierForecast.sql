WITH [Dates] AS (
	SELECT * FROM (
	VALUES 
		(@FromDate), 
		(DATEADD(dd, 1, @FromDate)), 
		(DATEADD(dd, 2, @FromDate)), 
		(DATEADD(dd, 3, @FromDate)), 
		(DATEADD(dd, 4, @FromDate)), 
		(DATEADD(dd, 5, @FromDate)), 
		(DATEADD(dd, 6, @FromDate)), 
		(DATEADD(dd, 7, @FromDate)), 
		(DATEADD(dd, 8, @FromDate)), 
		(DATEADD(dd, 9, @FromDate)), 
		(DATEADD(dd, 10, @FromDate)))
	AS X([Date])
),
[ProductQuantitiesPerDate] AS (
	SELECT
		[dt].[Date],
		[rop].[ProductId],
		SUM([rop].[Quantity]) AS [TotalQuantity]
	FROM [dbo].[RecurringOrders] AS [ro]
	INNER JOIN [dbo].[Timeslots] AS [ts] ON [ro].[TimeslotId] = [ts].[Id]

	INNER JOIN [Dates] AS [dt] ON 
		[ro].[StartDate] <= [dt].[Date]							-- Past the start date
		AND [ro].[EndDate] >= [dt].[Date]						-- Before the end date

	LEFT JOIN [dbo].[Holidays] AS [hd] ON
		[hd].[Date] = [dt].[Date]

	LEFT JOIN [dbo].[SkipDates] AS [sd] ON 
		[ro].[Id] = [sd].[RecurringOrderId] 
		AND [sd].[Date] = [dt].[Date]

	INNER JOIN [dbo].[RecurringOrderProducts] AS [rop] ON [ro].[Id] = [rop].[RecurringOrderId]

	WHERE
		[ro].[IsDeleted] = 0									-- Not deleted
		AND [ro].[EndDate] >= [ro].[StartDate]					-- Not deleted in old way
		AND [hd].[Date] IS NULL									-- No bank holiday
		AND [sd].[Date] IS NULL									-- Not skipped
		AND [ts].[DayOfWeek]  = (DATEPART(dw, [dt].[Date]) - 1)	-- Correct day of week
	GROUP BY
		[dt].[Date],
		[rop].[ProductId]
)
SELECT
	[prd].[ProductCode],
	[prd].[ProductGroup],
	[prd].[ProductName],
	[prd].[SupplierSKU],
	[d0].[TotalQuantity] AS [Day0],
	[d1].[TotalQuantity] AS [Day1],
	[d2].[TotalQuantity] AS [Day2],
	[d3].[TotalQuantity] AS [Day3],
	[d4].[TotalQuantity] AS [Day4],
	[d5].[TotalQuantity] AS [Day5],
	[d6].[TotalQuantity] AS [Day6],
	[d7].[TotalQuantity] AS [Day7],
	[d8].[TotalQuantity] AS [Day8],
	[d9].[TotalQuantity] AS [Day9],
	[d10].[TotalQuantity] AS [Day10]
FROM [dbo].[Products] AS [prd]
LEFT JOIN [ProductQuantitiesPerDate] AS [d0] ON [prd].[Id] = [d0].[ProductId] AND [d0].[Date] = @FromDate
LEFT JOIN [ProductQuantitiesPerDate] AS [d1] ON [prd].[Id] = [d1].[ProductId] AND [d1].[Date] = DATEADD(dd, 1, @FromDate)
LEFT JOIN [ProductQuantitiesPerDate] AS [d2] ON [prd].[Id] = [d2].[ProductId] AND [d2].[Date] = DATEADD(dd, 2, @FromDate)
LEFT JOIN [ProductQuantitiesPerDate] AS [d3] ON [prd].[Id] = [d3].[ProductId] AND [d3].[Date] = DATEADD(dd, 3, @FromDate)
LEFT JOIN [ProductQuantitiesPerDate] AS [d4] ON [prd].[Id] = [d4].[ProductId] AND [d4].[Date] = DATEADD(dd, 4, @FromDate)
LEFT JOIN [ProductQuantitiesPerDate] AS [d5] ON [prd].[Id] = [d5].[ProductId] AND [d5].[Date] = DATEADD(dd, 5, @FromDate)
LEFT JOIN [ProductQuantitiesPerDate] AS [d6] ON [prd].[Id] = [d6].[ProductId] AND [d6].[Date] = DATEADD(dd, 6, @FromDate)
LEFT JOIN [ProductQuantitiesPerDate] AS [d7] ON [prd].[Id] = [d7].[ProductId] AND [d7].[Date] = DATEADD(dd, 7, @FromDate)
LEFT JOIN [ProductQuantitiesPerDate] AS [d8] ON [prd].[Id] = [d8].[ProductId] AND [d8].[Date] = DATEADD(dd, 8, @FromDate)
LEFT JOIN [ProductQuantitiesPerDate] AS [d9] ON [prd].[Id] = [d9].[ProductId] AND [d9].[Date] = DATEADD(dd, 9, @FromDate)
LEFT JOIN [ProductQuantitiesPerDate] AS [d10] ON [prd].[Id] = [d10].[ProductId] AND [d10].[Date] = DATEADD(dd, 10, @FromDate)
WHERE
	[SupplierId] = @SupplierId
	AND [IsDeleted] = 0
ORDER BY
	[prd].[ProductCode]