SELECT [cu].[CompanyName],
       [co].[FirstName],
       [co].[LastName],
       [co].[Phone],
       [co].[Email],
       [d].[DeliveryDate],
	   CONVERT(varchar, [d].[EarliestTime], 100) + ' - ' + CONVERT(varchar, [d].[LatestTime], 100) AS Timeslot,
       [d].[ShortId] AS [DeliveryShortId],
       [op].[ProductName],
	   ISNULL([op].[SalePrice], [op].[Price]) AS UnitPrice,
       [ds].[DesiredQuantity],
       [ds].[ActualQuantity],
	   ISNULL(STUFF(
       (
           SELECT ', ' + CAST([da].[Quantity] AS VARCHAR) + ' x ' + CAST(TRIM([p2].[ProductName]) AS VARCHAR)
           FROM [dbo].[DeliveryAdditions] AS [da]
               INNER JOIN [dbo].[Products] AS [p2]
                   ON [p2].[Id] = [da].[ProductId]
           WHERE [da].[DeliveryShortageId] = [ds].[Id]
           FOR XML PATH('')
       ), 1, 2,''), 'N/A') AS [Replacement],
       [ds].[Reason],
       [ds].[ReasonCode]
FROM [dbo].[DeliveryShortages] AS [ds]
    INNER JOIN [dbo].[Orders] AS [o]
        ON [o].[Id] = [ds].[OrderId]
	INNER JOIN [dbo].[OrderProducts] AS [op] ON [op].[OrderId] = [ds].[OrderId] AND [op].[ProductId] = [ds].[ProductId]
    INNER JOIN [dbo].[Products] AS [p]
        ON [p].[Id] = [ds].[ProductId]
    INNER JOIN [dbo].[DeliveryOrders] AS [do]
        ON [do].[OrderId] = [o].[Id]
    INNER JOIN [dbo].[Deliveries] AS [d]
        ON [d].[Id] = [do].[DeliveryId]
    INNER JOIN [dbo].[Contacts] AS [co]
        ON [co].[Id] = [o].[ContactId]
    INNER JOIN [dbo].[Customers] AS [cu]
        ON [cu].[Id] = [co].[CustomerId]
	WHERE [d].[DeliveryDate] >= CAST(@FromDate AS DATETIME) 
	AND [d].[DeliveryDate] <= CAST(@ToDate AS DATETIME) 
ORDER BY [d].[EarliestTime], [cu].[CompanyName]