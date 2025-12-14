SELECT
	[d].*,
	JSON_QUERY((
		SELECT
			[od].*,
			CASE 
			WHEN EXISTS (SELECT [o2].Id 
							 FROM dbo.Orders AS o2 
							 WHERE o2.DeliveryDate < [od].DeliveryDate 
							 AND [o2].CustomerId = [od].CustomerId) 
			THEN 0 
			ELSE 1 END AS IsFirstOrder, 
			JSON_QUERY((
				SELECT * FROM [dbo].[OrderProducts]
				WHERE [OrderId] = [od].[Id]
				FOR JSON PATH
			)) AS [OrderItems],
			JSON_QUERY((
				SELECT *
				FROM [dbo].[OrderDiscounts]
				WHERE [OrderId] = [od].[Id]
				FOR JSON PATH
			)) AS [OrderDiscounts]
		FROM [dbo].[Orders] AS [od]
		INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [od].[Id] = [do].[OrderId]
		WHERE [do].[DeliveryId] = [d].[Id]
		FOR JSON PATH)) AS [Orders],

	JSON_QUERY((
		SELECT
			[ds].*,
			[prd].[ProductName],
			-- <Get all replacements for each shortage> --
				JSON_QUERY((
					SELECT
						[da].*,
						[prd].[ProductName],
						[prd].[ProductCode],
						ISNULL([prd].[SalePrice], [prd].[Price]) AS [ProductPrice],
						[prd].[IsTaxable]
					FROM [dbo].[DeliveryAdditions] AS [da]
					INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [da].[OrderId] = [do].[OrderId]
					LEFT JOIN [dbo].[Products] AS [prd] ON [prd].[Id] = [da].[ProductId]
					WHERE [da].[DeliveryShortageId] = [ds].[Id]
					FOR JSON PATH
				)) AS [Replacements]
				-- </Get all replacements for each shortage> --
		FROM [dbo].[DeliveryShortages] AS [ds]
		INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [ds].[OrderId] = [do].[OrderId]
		LEFT JOIN [dbo].[Products] AS [prd] ON [prd].[Id] = [ds].[ProductId]
		WHERE [do].[DeliveryId] = [d].[Id]
		FOR JSON PATH)) AS [DeliveryShortages],

	JSON_QUERY((
		SELECT
			[da].*,
			[prd].[ProductName],
			[prd].[ProductCode],
			ISNULL([prd].[SalePrice], [prd].[Price]) AS [ProductPrice],
			[prd].[IsTaxable]
		FROM [dbo].[DeliveryAdditions] AS [da]
		INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [da].[OrderId] = [do].[OrderId]
		LEFT JOIN [dbo].[Products] AS [prd] ON [prd].[Id] = [da].[ProductId]
		WHERE [do].[DeliveryId] = [d].[Id]
		FOR JSON PATH)) AS [DeliveryAdditions]

FROM [dbo].[Deliveries] AS [d]
WHERE 
	[d].[DeliveryDate] >= @FromDate 
	AND [d].[DeliveryDate] <= @ToDate
ORDER BY
	[d].[DeliveryDate] DESC,
	[d].[LatestTime] ASC,
	[d].[ShortId]