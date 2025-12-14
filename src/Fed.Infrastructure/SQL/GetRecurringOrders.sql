DECLARE @RecurringOrdersTemp TABLE
(
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[ContactId] [uniqueidentifier] NOT NULL,
	[DeliveryAddressId] [uniqueidentifier] NOT NULL,
	[BillingAddressId] [uniqueidentifier] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[WeeklyRecurrence] [int] NOT NULL,
	[TimeslotId] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[LastUpdatedDate] [datetime2](7) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsFree] [bit] NOT NULL
)

INSERT INTO @RecurringOrdersTemp
SELECT [ro].*
FROM [dbo].[RecurringOrders] AS [ro]
INNER JOIN [dbo].[Contacts] AS [co] ON [ro].[ContactId] = [co].[Id]
INNER JOIN [dbo].[Customers] AS [cu] ON [co].[CustomerId] = [cu].[Id]
INNER JOIN [dbo].[AccountTypes] AS [at] ON [cu].[AccountTypeId] = [at].[Id]
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
			ORDER BY [p].[ProductGroup], [p].[ProductName]
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
	FROM @RecurringOrdersTemp AS [ro]
	INNER JOIN [dbo].[Timeslots] AS [ts] ON [ro].[TimeslotId] = [ts].[Id]
	INNER JOIN [dbo].[Contacts] AS [co] ON [ro].[ContactId] = [co].[Id]
	INNER JOIN [dbo].[Customers] AS [cu] ON [co].[CustomerId] = [cu].[Id]
	ORDER BY 
		[ts].[LatestTime], 
		[cu].[CompanyName]
	FOR JSON PATH
) SELECT @JSON