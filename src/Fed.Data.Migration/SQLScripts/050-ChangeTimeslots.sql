BEGIN TRY
	BEGIN TRANSACTIOn

		-- |===================================|
		-- |    1. Generate new timeslots      |
		-- |===================================|

		DECLARE @HubId [uniqueidentifier]
		SET @HubId = (SELECT TOP 1 [ID] FROM [dbo].[Hubs])

		DECLARE @MondayId1 [uniqueidentifier]
		DECLARE @MondayId2 [uniqueidentifier]
		DECLARE @TuesdayId1 [uniqueidentifier]
		DECLARE @TuesdayId2 [uniqueidentifier]
		DECLARE @WednesdayId1 [uniqueidentifier]
		DECLARE @WednesdayId2 [uniqueidentifier]
		DECLARE @ThursdayId1 [uniqueidentifier]
		DECLARE @ThursdayId2 [uniqueidentifier]
		DECLARE @FridayId1 [uniqueidentifier]
		DECLARE @FridayId2 [uniqueidentifier]

		SET @MondayId1 = NEWID()
		SET @MondayId2 = NEWID()
		SET @TuesdayId1 = NEWID()
		SET @TuesdayId2 = NEWID()
		SET @WednesdayId1 = NEWID()
		SET @WednesdayId2 = NEWID()
		SET @ThursdayId1 = NEWID()
		SET @ThursdayId2 = NEWID()
		SET @FridayId1 = NEWID()
		SET @FridayId2 = NEWID()

		-- Monday

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@MondayId1, @HubId, 1, N'07:00', N'09:00', 30)

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@MondayId2, @HubId, 1, N'09:00', N'12:00', 30)

		-- Tuesday

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@TuesdayId1, @HubId, 2, N'07:00', N'09:00', 30)

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@TuesdayId2, @HubId, 2, N'09:00', N'12:00', 30)

		-- Wednesday

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@WednesdayId1, @HubId, 3, N'07:00', N'09:00', 30)

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@WednesdayId2, @HubId, 3, N'09:00', N'12:00', 30)

		-- Thursday

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@ThursdayId1, @HubId, 4, N'07:00', N'09:00', 30)

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@ThursdayId2, @HubId, 4, N'09:00', N'12:00', 30)

		-- Friday

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@FridayId1, @HubId, 5, N'07:00', N'09:00', 30)

		INSERT INTO [dbo].[Timeslots] ([Id], [HubId], [DayOfWeek], [EarliestTime], [LatestTime], [TotalCapacity])
		VALUES (@FridayId2, @HubId, 5, N'09:00', N'12:00', 30)

		-- |===================================|
		-- |    2. Change recurring orders     |
		-- |===================================|

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @MondayId1
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 1 AND [EarliestTime] = N'08:00:00' AND [LatestTime] = N'10:00:00')

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @MondayId2
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 1 AND [Id] <> @MondayId1)

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @TuesdayId1
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 2 AND [EarliestTime] = N'08:00:00' AND [LatestTime] = N'10:00:00')

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @TuesdayId2
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 2 AND [Id] <> @TuesdayId1)

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @WednesdayId1
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 3 AND [EarliestTime] = N'08:00:00' AND [LatestTime] = N'10:00:00')

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @WednesdayId2
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 3 AND [Id] <> @WednesdayId1)

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @ThursdayId1
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 4 AND [EarliestTime] = N'08:00:00' AND [LatestTime] = N'10:00:00')

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @ThursdayId2
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 4 AND [Id] <> @ThursdayId1)

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @FridayId1
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 5 AND [EarliestTime] = N'08:00:00' AND [LatestTime] = N'10:00:00')

		UPDATE [dbo].[RecurringOrders]
		SET [TimeslotId] = @FridayId2
		WHERE [TimeslotId] IN (SELECT [Id] FROM [dbo].[Timeslots] WHERE [DayOfWeek] = 5 AND [Id] <> @FridayId1)

		-- |===================================|
		-- |    3. Delete old timeslots        |
		-- |===================================|

		DELETE FROM [dbo].[Timeslots]
		WHERE [Id] NOT IN (
			@MondayId1,
			@MondayId2, 
			@TuesdayId1, 
			@TuesdayId2, 
			@WednesdayId1, 
			@WednesdayId2, 
			@ThursdayId1, 
			@ThursdayId2, 
			@FridayId1, 
			@FridayId2)

	COMMIT
END TRY
BEGIN CATCH
	IF @@TRANCOUNT > 0
		ROLLBACK
END CATCH