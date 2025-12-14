SELECT
	[d].[Id]	AS [DeliveryId]

FROM [dbo].[Deliveries] AS [d]
INNER JOIN [dbo].[DeliveryOrders]	AS [do]		ON [do].[DeliveryId] = [d].[Id]
INNER JOIN [dbo].[Orders]			AS [o]		ON [o].[Id] = [do].OrderId
INNER JOIN [dbo].[OrderProducts]	AS [p]		ON [p].[OrderId] = [o].[Id]

LEFT  JOIN [dbo].[CardTransactions]	AS [ctr]	ON [ctr].[DeliveryId] = [d].[Id] 
													AND [ctr].[CardTransactionStatusId] = 2		-- Find all paid card transactions

INNER JOIN [dbo].[Customers]		AS [cu]		ON [cu].[Id] = [o].[CustomerId]
													AND [cu].[AccountTypeId] <> 1				-- Exclude Internal customers (they don't get charged or invoiced)
													AND [cu].[AccountTypeId] <> 2				-- Exclude Presale customers (they don't get charged or invoiced)
													AND [cu].[AccountTypeId] <> 3				-- Exclude Demo customers (their orders never get processed)
													AND [cu].[AccountTypeId] <> 6				-- Exclude Paused customers (their orders never get processed)

WHERE [o].[DeliveryDate] >= DATEADD(DAY, -7, @DeliveryDate)		-- All payments between the delivery date and 7 days prior
      AND [o].[PaymentMethodId] = 1								-- Filter only card payments
      AND [ctr].[Id] IS NULL									-- Exclude all card transactions which are paid already
	  
	 GROUP BY [d].[Id]		