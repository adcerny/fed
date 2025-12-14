SELECT 
	[ts].*,  
	ISNULL([ts2].[TakenCapacity], 0) AS [TakenCapacity], 
	ISNULL([ts].[TotalCapacity] - [ts2].[TakenCapacity], [ts].[TotalCapacity]) AS [AvailableCapacity] 
FROM [dbo].[Timeslots] AS [ts] 
LEFT JOIN (
	SELECT  
		[ts].[Id], 
		COUNT([ro].[Id]) AS [TakenCapacity] 
	FROM [dbo].[Timeslots] AS [ts] 
	INNER JOIN [dbo].[RecurringOrders] AS [ro] ON [ts].[Id] = [ro].[TimeslotId] 
	WHERE 
		[ro].[IsDeleted] = 0 
		AND [ro].[EndDate] >= GETDATE()
	GROUP BY [ts].[Id] 
) AS [ts2] ON [ts].[Id] = [ts2].[Id] 
WHERE [ts].[Id] = @Id