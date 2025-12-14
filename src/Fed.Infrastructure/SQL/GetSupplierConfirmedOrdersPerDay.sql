SELECT
	[prd].[SupplierId],
	[prd].[SupplierSKU],
	[prd].[ProductCode],
	[prd].[ProductName],
	SUM([op].[Quantity]) AS [FedQuantity],
	CEILING(
		SUM(
			CAST(([op].[Quantity] * ISNULL([uc].[FedUnits], 1)) AS float) /
			CAST((ISNULL([uc].[SupplierUnits], 1)) AS float)
	)) AS [SupplierQuantity],
	COUNT([op].[ProductCode]) AS [CustomerCount],
	STRING_AGG([od].[CompanyName], ', ') AS [Customers]

FROM [dbo].[Products] AS [prd]

LEFT JOIN [dbo].[OrderProducts] AS [op] ON [op].[ProductId] = [prd].[Id]
INNER JOIN [dbo].[Orders] AS [od] ON [op].[OrderId] = [od].[Id]
INNER JOIN [dbo].[Customers] AS [cu] ON [od].[CustomerId]  = [cu].[Id]
LEFT JOIN [dbo].[ProductUnitConversions] AS [uc] ON [prd].[SupplierId] = [uc].[SupplierId] AND [prd].[SupplierSKU] = [uc].[SupplierSKU]

WHERE
	[od].[DeliveryDate] = @Date										-- Specify delivery date
	AND (@SupplierId IS NULL OR [prd].[SupplierId] = @SupplierId)	-- Filter by supplier if provided
	AND [cu].[AccountTypeId] <> 3			-- Exclude Demo customers
	AND [cu].[AccountTypeId] <> 4			-- Exclude Cancelled customers
	AND [cu].[AccountTypeId] <> 5           -- Exclude Deleted customers
	AND [cu].[AccountTypeId] <> 6           -- Exclude Paused customers

GROUP BY
	[prd].[SupplierSKU],
	[prd].[SupplierId],
	[prd].[ProductCode],
	[prd].[ProductName]

ORDER BY
	[prd].[SupplierId],
	[prd].[SupplierSKU]