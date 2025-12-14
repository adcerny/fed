WITH TotalSalesLastWeekCTE (CustomerId, TotalSalesLastWeek)
AS (SELECT [od].[CustomerId],
           SUM(ISNULL([op].[SalePrice], [op].[Price]) * [op].[Quantity]) AS [TotalSalesLastWeek]
    FROM [dbo].[OrderProducts] AS [op]
        INNER JOIN [dbo].[Orders] AS [od]
            ON [op].[OrderId] = [od].[Id]
    WHERE
        -- Start of previous week: Last Monday
        [DeliveryDate] >= ( SELECT DATEADD(wk, DATEDIFF(wk, 6, GETDATE()), 0))
        -- End of previous week: Last Friday
        AND [DeliveryDate] <= ( SELECT DATEADD(wk, DATEDIFF(wk, 6, GETDATE()), 4))
    GROUP BY [od].[CustomerId]),
     LifecyleStatusCTE (CustomerId, LifecycleStatus)
AS (SELECT [cu].[Id] AS [CustomerId],
           (CASE
                WHEN [cu].[FirstDeliveryDate] IS NULL THEN
                    'Prospect'
                ELSE
                    CASE
                        WHEN DATEDIFF(DAY, MAX([od].[DeliveryDate]), GETDATE()) > 30 THEN
                            'Lapsed'
                        ELSE
                            'Active'
                    END
            END
           ) AS [LifecycleStatus]
    FROM [dbo].[Customers] AS [cu]
        LEFT JOIN [dbo].[Orders] AS [od]
            ON [cu].[Id] = [od].[CustomerId]
    GROUP BY [cu].[Id],
             [cu].[FirstDeliveryDate])
SELECT [cu].[Id] AS [CustomerId],
       [cu].[ShortId] AS [Company ID],
       [cu].[CompanyName] AS [Company Name],
       [co].[Email] AS [Contact Email],
	   [da].[Postcode],
       [co].[IsMarketingConsented] AS [Marketing Consent],
       CASE
			WHEN [cu].[OfficeSizeMin] IS NULL AND [cu].[OfficeSizeMax] IS NOT NULL THEN
				CAST([cu].[OfficeSizeMax] AS VARCHAR(5))
			WHEN [cu].[OfficeSizeMin] IS NOT NULL AND [cu].[OfficeSizeMax] IS NOT NULL THEN
				CAST([cu].[OfficeSizeMin] AS VARCHAR(5)) + ' - ' + CAST([cu].[OfficeSizeMax] AS VARCHAR(5))
			ELSE
				CAST([cu].[OfficeSizeMin] AS VARCHAR(5)) + ' + '
		END AS [Office Size],
       [cu].[RegisterDate] AS [Date Registered],
       COUNT(DISTINCT [od].[Id]) AS [Total Deliveries],
       [cu].[FirstDeliveryDate] AS [First Delivery Date],
       MAX([od].[DeliveryDate]) AS [Last Delivery Date],
       SUM(ISNULL([op].[SalePrice], [op].[Price]) * [op].[Quantity]) AS [Total Sales To Date],
       [cte].[TotalSalesLastWeek] AS [Total Sales Last Week],
       [cte2].[LifecycleStatus] AS [Status],
       [at].[AccountType] AS [Account Type],
       [cu].[Source],
       [cu].[CancellationReason] AS [Cancellation Reason],
       [cu].[CustomerAgentId] AS [Agent],
	   [cu].[CustomerMarketingAttributeId] AS [Marketing Attribute]
FROM [dbo].[Customers] AS [cu]
    LEFT JOIN [dbo].[Orders] AS [od]
        ON [cu].[Id] = [od].[CustomerId]
    LEFT JOIN [dbo].[OrderProducts] AS [op]
        ON [od].[Id] = [op].[OrderId]
    LEFT JOIN [dbo].[Contacts] AS [co]
        ON [cu].[Id] = [co].[CustomerId]
    LEFT JOIN TotalSalesLastWeekCTE AS [cte]
        ON [cu].[Id] = [cte].[CustomerId]
    INNER JOIN LifecyleStatusCTE AS [cte2]
        ON [cu].[Id] = [cte2].[CustomerId]
    INNER JOIN [dbo].[AccountTypes] AS [at]
        ON [cu].[AccountTypeId] = [at].[Id]
	LEFT JOIN [dbo].[DeliveryAddresses] AS [da]
        ON [da].[ContactId] = [co].[Id] AND [da].[IsPrimary] = 1 AND [da].[IsDeleted] = 0
WHERE
	(@AccountTypeId IS NULL OR TRIM(@AccountTypeId) = '' OR [cu].[AccountTypeId] = @AccountTypeId)
	AND (@LifecycleStatus IS NULL OR TRIM(@LifecycleStatus) = '' OR @LifecycleStatus = [cte2].[LifecycleStatus])
GROUP BY [cu].[Id],
         [od].[CustomerId],
         [cu].[ShortId],
         [cu].[CompanyName],
         [co].[Email],
         [co].[IsMarketingConsented],
         [cu].[OfficeSizeMin],
         [cu].[OfficeSizeMax],
         [cu].[RegisterDate],
         [cu].[FirstDeliveryDate],
         [cte].[TotalSalesLastWeek],
         [cu].[IsInvoiceable],
         [cu].[OfficeSizeMin],
         [cu].[OfficeSizeMax],
         [cte2].[LifecycleStatus],
         [at].[AccountType],
         [cu].[Source],
         [cu].[CancellationReason],
         [cu].[CustomerAgentId],
		 [da].[Postcode],
		 [cu].[CustomerMarketingAttributeId]
ORDER BY [cu].[CompanyName];