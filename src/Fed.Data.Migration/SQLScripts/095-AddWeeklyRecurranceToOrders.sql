IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Orders]') AND [name] = 'WeeklyRecurrence')
BEGIN
    ALTER TABLE [dbo].[Orders] ADD [WeeklyRecurrence] [INT] NOT NULL DEFAULT 1
END

GO

UPDATE [o]
SET [o].[WeeklyRecurrence] = [ro].[WeeklyRecurrence]
FROM [dbo].[Orders] [o]
    INNER JOIN [dbo].[RecurringOrders] AS [ro]
        ON [ro].[Id] = [o].[RecurringOrderId];