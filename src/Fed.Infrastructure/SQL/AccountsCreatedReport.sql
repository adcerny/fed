DECLARE @MondayTwoWeeksAgo DATETIME2
SET @MondayTwoWeeksAgo = (SELECT DATEADD(wk, DATEDIFF(wk, 13, GETDATE()), 0));

WITH TempCTE (
	[CustomerId],
	[Company Name], 
	[Account Type], 
	[Office Size], 
	[Source], 
	[Email], 
	[Marketing Opt In], 
	[Register Date],
	[Account Setup], 
	[Next Delivery Date],
	[First Delivery Date], 
	[Agent]) 
AS
(
	SELECT
		[cu].[Id] AS [CustomerId],
		[cu].[CompanyName] AS [Company Name],
		[at].[AccountType] AS [Account Type],

		CAST([cu].[OfficeSizeMin] AS VARCHAR(5))
		+	CASE WHEN [cu].[OfficeSizeMax] IS NULL THEN ' +'
			ELSE ' - ' + CAST([cu].[OfficeSizeMax] AS VARCHAR(5))
			END
		AS [Office Size],
		[cu].[Source],
		[co].[Email],
		[co].[IsMarketingConsented] AS [Marketing Opt In],
		CAST([cu].[RegisterDate] AS DATE) AS [Register Date],
		(CASE WHEN [ba].[Id] IS NULL THEN '' ELSE 'Complete' END) AS [Account Setup],
		[cu].[FirstDeliveryDate] AS [First Delivery Date],
		CAST(DATEADD(dd, 8 - DATEPART(dw, GETDATE()) + [tsx].[DayOfWeek], GETDATE()) AS DATE) AS [Next Delivery Date],
		[cu].[CustomerAgentId] AS [Agent]
	FROM [dbo].[Customers]				AS [cu]
	INNER JOIN [dbo].[Contacts]			AS [co]  ON [cu].[Id]			= [co].[CustomerId]
	INNER JOIN [dbo].[AccountTypes]		AS [at]  ON [cu].[AccountTypeId] = [at].[Id]
	LEFT JOIN [dbo].[RecurringOrders]	AS [ro]  ON [co].[Id]			= [ro].[ContactId]
	LEFT JOIN [dbo].[Timeslots]			AS [ts]  ON [ro].[TimeslotId]	= [ts].[Id]
	LEFT JOIN [dbo].[BillingAddresses]  AS [ba]  ON [co].[Id]			= [ba].[ContactId]

	LEFT JOIN [dbo].[RecurringOrders]	AS [rox] ON [co].[Id]			= [rox].[ContactId]
												 AND [rox].[EndDate] >= GETDATE()
												 AND [rox].[IsDeleted] = 0
	LEFT JOIN [dbo].[Timeslots]			AS [tsx] ON [rox].[TimeslotId]	= [tsx].[Id]

	WHERE
		[cu].[RegisterDate] >= 	@MondayTwoWeeksAgo

	GROUP BY
		[cu].[Id],
		[cu].[CompanyName],
		[at].[AccountType],
		[cu].[OfficeSizeMin],
		[cu].[OfficeSizeMax],
		[cu].[Source],
		[co].[Email],
		[co].[IsMarketingConsented],
		[cu].[RegisterDate],
		[ba].[Id],
		[cu].[FirstDeliveryDate],
		[tsx].[DayOfWeek],
		[cu].[CustomerAgentId]
)
SELECT
	[t1].[CustomerId], 
	[t1].[Company Name], 
	[t1].[Account Type], 
	[t1].[Office Size], 
	[t1].[Source], 
	[t1].[Email], 
	[t1].[Marketing Opt In], 
	[t1].[Register Date], 
	[t1].[Account Setup],
	[t1].[Next Delivery Date],
	[t1].[First Delivery Date], 
	[t1].[Agent]
FROM TempCTE AS [t1]
LEFT JOIN
(
	SELECT
		[CustomerId], 
		MIN([Next Delivery Date]) AS [Next Delivery Date] 
	FROM TempCTE 
	GROUP BY [CustomerId]
)
AS [t2] ON [t1].[CustomerId] = [t2].[CustomerId]
WHERE 
	[t1].[Account Type] <> 'Deleted'
	AND ([t1].[Next Delivery Date] IS NULL 
	OR [t1].[Next Delivery Date] = [t2].[Next Delivery Date])
ORDER BY
	[t1].[Register Date] DESC,
	[t1].[Company Name] ASC