SELECT
	[da].*,
	[p].[ProductName],
	[p].[ProductCode],
	ISNULL([p].[SalePrice], [p].[Price]) AS [ProductPrice],
	[p].[IsTaxable]
FROM [dbo].[DeliveryAdditions] AS [da]
LEFT JOIN [dbo].[Products] AS [p] ON [da].[ProductId] = [p].[Id]
WHERE [da].[Id] = @Id