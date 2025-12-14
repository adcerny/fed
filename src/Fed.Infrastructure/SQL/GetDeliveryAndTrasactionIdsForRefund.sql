SELECT
	[d].[Id]				AS [DeliveryId]	,
	[payments].[Id]			AS [OriginalCardTransactionId]
FROM [dbo].[DeliveryShortages]		AS [ds]
INNER JOIN [dbo].[Orders]			AS [o]			ON [o].[Id] = [ds].[OrderId]
INNER JOIN [dbo].[DeliveryOrders]	AS [do]			ON [do].[OrderId] = [o].[Id]
INNER JOIN [dbo].[Deliveries]		AS [d]			ON [d].[Id] = [do].[DeliveryId]
CROSS APPLY																						   -- Use cross apply to ensure only one transaction is selected
        (
        SELECT  TOP(1) *
        FROM    [dbo].[CardTransactions] AS [p]
        WHERE   [p].[DeliveryId] = [d].[Id] 
		AND  [p].[CardTransactionStatusId] = 2
		ORDER BY [p].[AmountCaptured] DESC
        ) [payments]
LEFT JOIN  [dbo].[CardTransactions]	AS [refunds]	ON [refunds].[DeliveryId] = [d].[Id] 
														AND [refunds].[CardTransactionStatusId] = 4
INNER JOIN [dbo].[Customers]		AS [cu]			ON [cu].[Id] = [o].[CustomerId]
														AND [cu].[AccountTypeId] <> 1			-- Exclude Internal customers (they don't get charged or invoiced)
														AND [cu].[AccountTypeId] <> 2			-- Exclude Presale customers (they don't get charged or invoiced)
														AND [cu].[AccountTypeId] <> 3			-- Exclude Demo customers (their orders never get processed)
														AND [cu].[AccountTypeId] <> 6			-- Exclude Paused customers (their orders never get processed)

WHERE 
[d].[DeliveryDate] >= DATEADD(DAY, -6, @DeliveryDate)
AND 
[refunds].[Id] IS NULL 
AND [o].[PaymentMethodId] = 1										-- Only apply automatic refunds for card payments
	
GROUP BY [d].[Id],
	     [payments].[Id]
