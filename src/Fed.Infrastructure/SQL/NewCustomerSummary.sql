DECLARE @Monday1		DATETIME2
DECLARE @Monday2		DATETIME2
DECLARE @Monday3		DATETIME2
DECLARE @Monday4		DATETIME2

DECLARE @WeekNumber1 INT
DECLARE @WeekNumber2 INT
DECLARE @WeekNumber3 INT
DECLARE @WeekNumber4 INT

SET @Monday1		= (SELECT DATEADD(wk, DATEDIFF(wk, 20, GETDATE()), 0))
SET @Monday2		= (SELECT DATEADD(wk, DATEDIFF(wk, 13, GETDATE()), 0))
SET @Monday3		= (SELECT DATEADD(wk, DATEDIFF(wk, 6, GETDATE()), 0))
SET @Monday4		= (SELECT DATEADD(wk, DATEDIFF(wk, 0, GETDATE()), 0))

SET @WeekNumber1 = DATEPART(ISO_WEEK, @Monday1)
SET @WeekNumber2 = DATEPART(ISO_WEEK, @Monday2)
SET @WeekNumber3 = DATEPART(ISO_WEEK, @Monday3)
SET @WeekNumber4 = DATEPART(ISO_WEEK, @Monday4)

SELECT
	[cu].[Id],
	[cu].[CompanyName],

	CAST([cu].[OfficeSizeMin] AS VARCHAR(5))
	+	CASE WHEN [cu].[OfficeSizeMax] IS NULL THEN ' +'
		ELSE ' - ' + CAST([cu].[OfficeSizeMax] AS VARCHAR(5))
		END
	AS [Office Size],

	SUM((ISNULL([op].[SalePrice], [op].[Price]) * [op].[Quantity])) AS [Order Value],

	(CASE WHEN DATEPART(ISO_WEEK, [cu].[FirstDeliveryDate]) = @WeekNumber1
	THEN [cu].[FirstDeliveryDate] ELSE NULL END) AS [Week-3],

	(CASE WHEN DATEPART(ISO_WEEK, [cu].[FirstDeliveryDate]) = @WeekNumber2
	THEN [cu].[FirstDeliveryDate] ELSE NULL END) AS [Week-2],

	(CASE WHEN DATEPART(ISO_WEEK, [cu].[FirstDeliveryDate]) = @WeekNumber3
	THEN [cu].[FirstDeliveryDate] ELSE NULL END) AS [Week-1],

	(CASE WHEN DATEPART(ISO_WEEK, [cu].[FirstDeliveryDate]) = @WeekNumber4
	THEN [cu].[FirstDeliveryDate] ELSE NULL END) AS [Week0]

FROM [dbo].[Customers]				AS [cu]
INNER JOIN [dbo].[Orders]			AS [od] ON [cu].[Id] = [od].[CustomerId]
INNER JOIN [dbo].[OrderProducts]	AS [op] ON [od].[Id] = [op].[OrderId]
INNER JOIN [dbo].[DeliveryOrders]	AS [do] ON [od].[Id] = [do].[OrderId]
INNER JOIN [dbo].[Deliveries]		AS [dl] ON [dl].[Id] = [do].[DeliveryId]
WHERE
	[cu].[AccountTypeId] = 0							-- All "Standard" customers
	AND [cu].[FirstDeliveryDate] >= @Monday1			-- All customers who had their 1st delivery since last week
	AND [dl].[DeliveryDate] = [cu].[FirstDeliveryDate]	-- Only examine their first delivery
GROUP BY
	[cu].[Id],
	[cu].[CompanyName],
	[cu].[OfficeSizeMin],
	[cu].[OfficeSizeMax],
	[cu].[FirstDeliveryDate]
ORDER BY
	[cu].[FirstDeliveryDate] DESC,
	[cu].[CompanyName] ASC