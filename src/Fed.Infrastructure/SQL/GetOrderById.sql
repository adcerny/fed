DECLARE @JSON NVARCHAR(MAX) = (
	SELECT 
		[od].*,
		JSON_QUERY((
			SELECT * FROM [dbo].[OrderProducts]
			WHERE [OrderId] = [od].[Id]
			ORDER BY [ProductGroup], [ProductName]
			FOR JSON PATH
		)) AS [OrderItems],
		JSON_QUERY((
            SELECT *
            FROM [dbo].[OrderDiscounts]
            WHERE [OrderId] = [od].[Id]
            FOR JSON PATH
        )) AS [OrderDiscounts]

	FROM [dbo].[Orders] AS [od]
	WHERE [od].[Id] = @Id
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
) SELECT @JSON