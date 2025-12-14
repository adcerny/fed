
WITH
	-- //// NO PRIMARY DELIVERY ADDRESS /// --

	NonPrimaryDeliveryAddresses ([DeliveryAddressId], [ContactId])
	AS
	(
		SELECT [Id], [ContactId] FROM [dbo].[DeliveryAddresses] WHERE [IsPrimary] = 0
	),
	ContactsWithNonPrimaryDeliveryAddresses ([ContactId], [AddressCount])
	AS
	(
		SELECT
			[co].[Id] AS [ContactId],
			COUNT([co].[Id]) AS [AddressCount]
		FROM [dbo].[Customers] AS [cu]
		INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
		INNER JOIN [dbo].[DeliveryAddresses] AS [da] ON [co].[Id] = [da].[ContactId]
		INNER JOIN [NonPrimaryDeliveryAddresses] AS [npda] ON [da].[Id] = [npda].[DeliveryAddressId]
		GROUP BY
			[co].[Id]
	),

	-- //// TOO MANY PRIMARY DELIVERY ADDRESSES /// --

	PrimaryDeliveryAddresses ([ContactId], [AddressCount])
	AS
	(
		SELECT
			[co].[Id] AS [ContactId],
			COUNT([co].[Id]) AS [AddressCount]
		FROM [dbo].[Customers] AS [cu]
		INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
		INNER JOIN [dbo].[DeliveryAddresses] AS [da] ON [co].[Id] = [da].[ContactId]
		WHERE [da].[IsPrimary] = 1
		GROUP BY
			[co].[Id]
	),

	-- //// NO PRIMARY BILLING ADDRESS /// --

	NonPrimaryBillingAddresses ([BillingAddressId], [ContactId])
	AS
	(
		SELECT [Id], [ContactId] FROM [dbo].[BillingAddresses] WHERE [IsPrimary] = 0
	),
	ContactsWithNonPrimaryBillingAddresses ([ContactId], [AddressCount])
	AS
	(
		SELECT
			[co].[Id] AS [ContactId],
			COUNT([co].[Id]) AS [AddressCount]
		FROM [dbo].[Customers] AS [cu]
		INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
		INNER JOIN [dbo].[BillingAddresses] AS [ba] ON [co].[Id] = [ba].[ContactId]
		INNER JOIN [NonPrimaryBillingAddresses] AS [npba] ON [ba].[Id] = [npba].[BillingAddressId]
		GROUP BY
			[co].[Id]
	),

	-- //// TOO MANY PRIMARY BILLING ADDRESSES /// --

	PrimaryBillingAddresses ([ContactId], [AddressCount])
	AS
	(
		SELECT
			[co].[Id] AS [ContactId],
			COUNT([co].[Id]) AS [AddressCount]
		FROM [dbo].[Customers] AS [cu]
		INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
		INNER JOIN [dbo].[BillingAddresses] AS [ba] ON [co].[Id] = [ba].[ContactId]
		WHERE [ba].[IsPrimary] = 1
		GROUP BY
			[co].[Id]
	),
	
	-- //// NO PRIMARY CARD TOKENS /// --

	NonPrimaryCardTokens ([CardTokenId], [ContactId])
	AS
	(
		SELECT [Id], [ContactId] FROM [dbo].[CardTokens] WHERE [IsPrimary] = 0
	),
	ContactsWithNonPrimaryCardTokens ([ContactId], [CardTokenCount])
	AS
	(
		SELECT
			[co].[Id] AS [ContactId],
			COUNT([co].[Id]) AS [CardTokenCount]
		FROM [dbo].[Customers] AS [cu]
		INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
		INNER JOIN [dbo].[CardTokens] AS [ct] ON [co].[Id] = [ct].[ContactId]
		INNER JOIN [NonPrimaryCardTokens] AS [npct] ON [ct].[Id] = [npct].[CardTokenId]
		GROUP BY
			[co].[Id]
	),

	-- //// TOO MANY PRIMARY CARD TOKENS /// --

	PrimaryCardTokens ([ContactId], [CardTokenCount])
	AS
	(
		SELECT
			[co].[Id] AS [ContactId],
			COUNT([co].[Id]) AS [CardTokenCount]
		FROM [dbo].[Customers] AS [cu]
		INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
		INNER JOIN [dbo].[CardTokens] AS [ct] ON [co].[Id] = [ct].[ContactId]
		WHERE [ct].[IsPrimary] = 1
		GROUP BY
			[co].[Id]
	)

-- NO PRIMARY DELIVERY ADDRESS

SELECT
	'No Primary Delivery Address' AS [Type],
	[cu].[CompanyName] + ' (ID: ' + [cu].[ShortId] + ') has no primary delivery address.' AS [Message]
FROM [dbo].[Customers] AS [cu]
INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
INNER JOIN [ContactsWithNonPrimaryDeliveryAddresses] AS [co2] ON [co].[Id] = [co2].[ContactId]
LEFT JOIN [dbo].[DeliveryAddresses] AS [da] ON [co].[Id] = [da].[ContactId] AND [da].[IsPrimary] = 1
WHERE [co2].[AddressCount] >= 1 AND [da].[Id] IS NULL

-- NO PRIMARY BILLING ADDRESS

UNION

SELECT
	'No Primary Billing Address' AS [Type],
	[cu].[CompanyName] + ' (ID: ' + [cu].[ShortId] + ') has no primary billing address.' AS [Message]
FROM [dbo].[Customers] AS [cu]
INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
INNER JOIN [ContactsWithNonPrimaryBillingAddresses] AS [co2] ON [co].[Id] = [co2].[ContactId]
LEFT JOIN [dbo].[BillingAddresses] AS [ba] ON [co].[Id] = [ba].[ContactId] AND [ba].[IsPrimary] = 1
WHERE [co2].[AddressCount] >= 1 AND [ba].[Id] IS NULL

-- NO PRIMARY CARD TOKEN

UNION

SELECT
	'No Primary Card Tokens' AS [Type],
	[cu].[CompanyName] + ' (ID: ' + [cu].[ShortId] + ') has no primary card token.' AS [Message]
FROM [dbo].[Customers] AS [cu]
INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
INNER JOIN [ContactsWithNonPrimaryCardTokens] AS [co2] ON [co].[Id] = [co2].[ContactId]
LEFT JOIN [dbo].[CardTokens] AS [ct] ON [co].[Id] = [ct].[ContactId] AND [ct].[IsPrimary] = 1
WHERE [co2].[CardTokenCount] >= 1 AND [ct].[Id] IS NULL

-- TOO MANY DELIVERY ADDRESSES

UNION

SELECT
	'Too Many Primary Delivery Addresses' AS [Type],
	[cu].[CompanyName] + ' (ID: ' + [cu].[ShortId] + ') has ' + CAST(([pda].[AddressCount]) AS [nvarchar](10)) + ' primary delivery addresses.' AS [Message]
FROM [dbo].[Customers] AS [cu]
INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
INNER JOIN [PrimaryDeliveryAddresses] AS [pda] ON [co].[Id] = [pda].[ContactId]
WHERE [pda].[AddressCount] > 1

-- TOO MANY BILLING ADDRESSES

UNION

SELECT
	'Too Many Primary Billing Addresses' AS [Type],
	[cu].[CompanyName] + ' (ID: ' + [cu].[ShortId] + ') has ' + CAST(([pba].[AddressCount]) AS [nvarchar](10)) + ' primary delivery addresses.' AS [Message]
FROM [dbo].[Customers] AS [cu]
INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
INNER JOIN [PrimaryBillingAddresses] AS [pba] ON [co].[Id] = [pba].[ContactId]
WHERE [pba].[AddressCount] > 1

-- TOO MANY CARD TOKENS

UNION

SELECT
	'Too Many Primary Card Tokens' AS [Type],
	[cu].[CompanyName] + ' (ID: ' + [cu].[ShortId] + ') has ' + CAST(([pct].[CardTokenCount]) AS [nvarchar](10)) + ' primary card tokens.' AS [Message]
FROM [dbo].[Customers] AS [cu]
INNER JOIN [dbo].[Contacts] AS [co] ON [cu].[Id] = [co].[CustomerId]
INNER JOIN [PrimaryCardTokens] AS [pct] ON [co].[Id] = [pct].[ContactId]
WHERE [pct].[CardTokenCount] > 1

-- FAULTY ORDERS

UNION

SELECT
	'Unexpected Order' AS [Type],
	[cu].[CompanyName] + ' (ID: ' + [cu].[ShortId] + ', AccountType: ' + [at].[AccountType] + ', IsTestAccount: ' 
	+ CAST(([cu].[IsTestAccount]) AS [nvarchar](5)) + ') has a confirmed delivery (Order name: ' + [od].[OrderName] 
	+ ') scheduled for ' + CAST(([od].[DeliveryDate]) AS nvarchar(50)) AS [Message]
FROM		[dbo].[Orders]			AS [od]
INNER JOIN	[dbo].[Contacts]		AS [co] ON [od].[ContactId]		= [co].[Id]
INNER JOIN	[dbo].[Customers]		AS [cu] ON [co].[CustomerId]	= [cu].[Id]
INNER JOIN	[dbo].[AccountTypes]	AS [at] ON [cu].[AccountTypeId] = [at].[Id]
WHERE
	[od].[DeliveryDate] >= GETDATE()
	AND 
	(
		[at].[AccountType]		= 'Deleted'
		OR [at].[AccountType]	= 'Cancelled'
		OR [at].[AccountType]	= 'Demo'
		OR [at].[AccountType]	= 'Paused'
		OR [cu].[IsTestAccount] = 1
	)