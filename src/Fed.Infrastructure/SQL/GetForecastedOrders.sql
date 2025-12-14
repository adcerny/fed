SELECT [ro].Id AS RecurringOrderId,
       [ro].[WeeklyRecurrence],
       [ro].[StartDate],
       [ro].[EndDate],
       [ro].[DeliveryAddressId],
       [ro].[CreatedDate],
       [co].[CustomerId],
       [co].[Id] AS [ContactId],
	   [t].[Id] AS [TimeslotId],
       [t].[DayOfWeek],
       MAX([o].[DeliveryDate]) AS [LastDeliveryDate],
	   [cu].[SplitDeliveriesByOrder]
FROM [dbo].[RecurringOrders] AS [ro]
    INNER JOIN [dbo].[Contacts] AS [co]
        ON [ro].[ContactId] = [co].[Id]
    INNER JOIN [dbo].[Customers] AS [cu]
        ON [co].[CustomerId] = [cu].[Id]
    INNER JOIN [dbo].[AccountTypes] AS [at]
        ON [cu].[AccountTypeId] = [at].[Id]
    INNER JOIN [dbo].[Timeslots] AS [t]
        ON [ro].[TimeslotId] = [t].[Id]
    INNER JOIN [dbo].[RecurringOrderProducts] AS [rop]
        ON [rop].[RecurringOrderId] = [ro].[Id]
           AND [rop].[Quantity] > 0
    LEFT JOIN [dbo].[Orders] AS [o]
        ON o.[RecurringOrderId] = [ro].[Id]
	WHERE
		(ISNULL(@IncludeFromDeletedAccounts, 0) = 1 OR [at].[AccountType] <> 'Deleted')
		AND (ISNULL(@IncludeFromCancelledAccounts, 0) = 1 OR [at].[AccountType] <> 'Cancelled')
		AND (ISNULL(@IncludeFromDemoAccounts, 0) = 1 OR [at].[AccountType] <> 'Demo')
        AND (ISNULL(@IncludeFromPausedAccounts, 0) = 1 OR [at].[AccountType] <> 'Paused')
		AND [ro].[IsDeleted] = 0 
		AND [ro].[StartDate] <= @ToDate
		AND [ro].[EndDate] >= @FromDate
		AND (ISNULL(@IncludeExpired, 0) = 1 OR [ro].[EndDate] >= GETDATE())
		AND (@ContactId IS NULL OR @ContactId = '00000000-0000-0000-0000-000000000000' OR @ContactId = [ro].[ContactId])
GROUP BY [ro].Id,
         [ro].[WeeklyRecurrence],
         [ro].[StartDate],
         [ro].[EndDate],
         [ro].[DeliveryAddressId],
         [ro].[CreatedDate],
         [co].[CustomerId],
         [co].[Id],
		 [t].[Id],
         [t].[DayOfWeek],
		 [cu].[SplitDeliveriesByOrder]