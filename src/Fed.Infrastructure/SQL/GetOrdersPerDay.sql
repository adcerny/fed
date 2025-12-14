SELECT
	[DeliveryDate],
	COUNT(*) AS 'Number of Orders'
FROM [dbo].[Orders]
WHERE
	[CompanyName] <> 'Fed by Abel and Cole'
	AND [CompanyName] <> 'Fed Buffer - DO NOT DELIVER'
	AND [CompanyName] <> 'Fed Test Account'
GROUP BY [DeliveryDate]
ORDER BY [DeliveryDate]