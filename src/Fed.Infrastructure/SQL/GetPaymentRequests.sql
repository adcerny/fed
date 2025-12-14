--************* Payments *******************--

SELECT
	[d].[Id]	AS [DeliveryId],
    [ct].[Id]	AS [CardTokenId],
    @CardTransactionBatchId AS [CardTransactionBatchId],
	NULL AS [OriginalCardTransactionId],
    (SUM(ISNULL([p].[SalePrice], [p].[Price]) * [p].[Quantity]) 
		+ 
		[d].[DeliveryCharge] --Add delivery charge
		-
		ISNULL([dis].Discount, 0)) --Subtract discount
		AS [Amount]

FROM [dbo].[Deliveries] AS [d]
INNER JOIN [dbo].[DeliveryOrders]	AS [do]		ON [do].[DeliveryId] = [d].[Id]
INNER JOIN [dbo].[Orders]			AS [o]		ON [o].[Id] = [do].OrderId
INNER JOIN [dbo].[OrderProducts]	AS [p]		ON [p].[OrderId] = [o].[Id]
LEFT JOIN (
		   SELECT [do2].[DeliveryId], 
		   SUM([od].[OrderTotalDeduction]) AS Discount                                 
		   FROM [dbo].[OrderDiscounts] AS [od] 
		   INNER JOIN [dbo].[DeliveryOrders] AS [do2]
		   ON [do2].[OrderId] = [od].[OrderId]
		   GROUP BY [do2].[DeliveryId]
		   ) 
		                            AS [dis] ON [d].Id = [dis].[DeliveryId]
LEFT  JOIN [dbo].[CardTransactions]	AS [ctr]	ON [ctr].[DeliveryId] = [d].[Id] 
													AND [ctr].[CardTransactionStatusId] = 2		-- Find all paid card transactions
INNER JOIN [dbo].[CardTokens]		AS [ct]		ON [ct].[ContactId] = [o].[ContactId]			-- Find the most recent card token for the given contact
													AND [ct].[IsPrimary] = 1
													AND [ct].[CreatedDate] =
													(
														SELECT MAX([CreatedDate])
														FROM [dbo].[CardTokens]
														WHERE [ContactId] = [o].[ContactId] AND [IsPrimary] = 1
													)
INNER JOIN [dbo].[Customers]		AS [cu]		ON [cu].[Id] = [o].[CustomerId]
													AND [cu].[AccountTypeId] <> 1				-- Exclude Internal customers (they don't get charged or invoiced)
													AND [cu].[AccountTypeId] <> 2				-- Exclude Presale customers (they don't get charged or invoiced)
													AND [cu].[AccountTypeId] <> 3	
													-- Exclude Demo customers (their orders never get processed)

WHERE [o].[DeliveryDate] >= DATEADD(DAY, -7, @DeliveryDate)										-- All payments between the delivery date and 7 days prior
      AND [o].[PaymentMethodId] = 1																-- Filter only card payments
      AND [ctr].[Id] IS NULL																	-- Exclude all card transactions which are paid already

GROUP BY [d].[Id],
		 [d].[DeliveryCharge],
         [ct].[Id],
		 [dis].[Discount]

UNION ALL

--************* Refunds *******************--

SELECT
	[d].[Id]				AS [DeliveryId],
    [ct].[Id]				AS [CardTokenId],
	@CardTransactionBatchId AS [CardTransactionBatchId],
    [payments].[Id]			AS [OriginalCardTransactionId],
    - SUM(ISNULL([op].[SalePrice], [op].[Price]) * ([ds].[DesiredQuantity] - [ds].[ActualQuantity])) AS [Amount]

FROM [dbo].[DeliveryShortages]		AS [ds]
INNER JOIN [dbo].[Orders]			AS [o]			ON [o].[Id] = [ds].[OrderId]
INNER JOIN [dbo].[DeliveryOrders]	AS [do]			ON [do].[OrderId] = [o].[Id]
INNER JOIN [dbo].[Deliveries]		AS [d]			ON [d].[Id] = [do].[DeliveryId]
INNER JOIN [dbo].[OrderProducts]	AS [op]			ON [op].[OrderId] = [o].[Id] 
														AND [op].[ProductId] = [ds].[ProductId]
INNER JOIN [dbo].[CardTokens]		AS [ct]			ON [ct].[ContactId] = [o].[ContactId]
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

WHERE [refunds].[Id] IS NULL AND [o].[PaymentMethodId] = 1										-- Only apply automatic refunds for card payments
	
GROUP BY [d].[Id],
         [d].[DeliveryCharge],
         [ct].[Id],
         [payments].[Id]