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
		AND ([ts].[HubId] = @HubId OR @AllHubs = 1)
	GROUP BY [ts].[Id] 
) AS [ts2] ON [ts].[Id] = [ts2].[Id] 
WHERE ([ts].[HubId] = @HubId OR @AllHubs = 1) AND ((@OnlyAvailableTimeslots = 0) OR (([ts].[TotalCapacity] - [ts2].[TakenCapacity]) > 0) OR [ts2].[TakenCapacity] IS NULL)
ORDER BY [DayOfWeek], [EarliestTime], [LatestTime]