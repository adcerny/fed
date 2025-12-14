DECLARE @JSON NVARCHAR(MAX) = (
	SELECT
		[od].[Id],
		[od].[DeliveryDate],
		[od].[OrderName],
		[od].[TimeslotId],
		[od].[EarliestTime],
		[od].[LatestTime],
		[od].[OrderGeneratedDate] AS DateAdded,
		[od].[WeeklyRecurrence],
		SUM(ISNULL([op].[SalePrice], [op].[Price]) * [op].[Quantity]) 
		-
		ISNULL((SELECT SUM([d].[OrderTotalDeduction]) --subtract any discounts
		FROM [dbo].[OrderDiscounts] AS [d] 
		WHERE [d].[OrderId] = [od].[Id]), 0) 
		AS [TotalPrice],
		JSON_QUERY((
			SELECT 
				[p].[IconCategory] AS [Icon],
				COUNT([p].[IconCategory]) AS [LineItemCount],
				SUM ([op].[Quantity]) AS [TotalQuantity]
			FROM [dbo].[OrderProducts] AS [op]
			INNER JOIN [dbo].[Products] AS [p] ON [op].[ProductId] = [p].[Id]
			WHERE [op].[OrderId] = [od].[Id]
			GROUP BY
				[p].[IconCategory]
			FOR JSON PATH
		)) AS [CategoryIcons]
	FROM [dbo].[Orders] AS [od]
	INNER JOIN [dbo].[OrderProducts] AS [op] ON [od].[Id] = [op].[OrderId] AND [op].[Quantity] > 0
	WHERE
		[od].[ContactId] = @ContactId
		AND [od].[DeliveryDate] >= @FromDate
		AND [od].[DeliveryDate] <= @ToDate
	GROUP BY
		[od].[Id],
		[od].[DeliveryDate],
		[od].[OrderName],
		[od].[TimeslotId],
		[od].[EarliestTime],
		[od].[LatestTime],
		[od].[OrderGeneratedDate],
		[od].[WeeklyRecurrence]
	ORDER BY [DateAdded]
	FOR JSON PATH
) SELECT @JSON