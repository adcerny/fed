DECLARE @JSON NVARCHAR(MAX) = (
	SELECT 
		[ro].*,
		JSON_QUERY((
			SELECT * FROM [dbo].[Timeslots]
			WHERE [Id] = [ro].[TimeslotId]
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
		)) AS [Timeslot],
		JSON_QUERY((
			SELECT * FROM [dbo].[DeliveryAddresses]
			WHERE [Id] = [ro].[DeliveryAddressId]
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
		)) AS [DeliveryAddress],
		JSON_QUERY((
			SELECT * FROM [dbo].[RecurringOrderProducts] AS [rop]
			LEFT JOIN [dbo].[Products] AS [p] ON [p].[Id] = [rop].[ProductId]
			WHERE [rop].[RecurringOrderId] = [ro].[Id]
			FOR JSON PATH
		)) AS [OrderItems],
		JSON_QUERY((
			SELECT * FROM [dbo].[SkipDates] WHERE [RecurringOrderId] = [ro].[Id]
			FOR JSON PATH
		)) AS [SkipDates],
		JSON_QUERY((
			SELECT * FROM [dbo].[Holidays]
			WHERE [Date] >= GETDATE()
			FOR JSON PATH
		)) AS [FutureHolidays]
	FROM [dbo].[RecurringOrders] AS [ro]
	INNER JOIN [dbo].[Timeslots] AS [ts] ON [ro].[TimeslotId] = [ts].[Id]
	INNER JOIN @ut t ON t.Id = [ro].[Id]
	WHERE 
		[ro].[IsDeleted] = 0 
	FOR JSON PATH
) SELECT @JSON
