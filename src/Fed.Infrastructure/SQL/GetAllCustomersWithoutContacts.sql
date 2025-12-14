DECLARE @JSON NVARCHAR(MAX) = (
SELECT
	 [Id]
	,[ShortId]
	,[CompanyName]
	,[Website]
	,[ACAccountNumber]
	,[IsInvoiceable]
	,[OfficeSizeMin]
	,[OfficeSizeMax]
	,[IsDeliveryChargeExempt]
	,[SplitDeliveriesByOrder]
	,[IsTestAccount]
	,[AccountTypeId]
	,[Source]
	,[Notes]
	,[IsFriend]
	,[CancellationReason]
	,[RegisterDate]
	,[FirstDeliveryDate]
	,(
		SELECT MAX([DeliveryDate])
		FROM [dbo].[Deliveries] AS [d]
		INNER JOIN [dbo].[Contacts] AS [co] ON [d].[ContactId] = [co].[Id]
		WHERE [co].[CustomerId] = [cu].[Id]
	 ) AS [LastDeliveryDate],
	 [CustomerAgentId],
	JSON_QUERY((
		SELECT * FROM [dbo].[CustomerAgents] AS [ca] WHERE [ca].[Id] = [cu].[CustomerAgentId] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)) AS CustomerAgent,
	JSON_QUERY((
		SELECT * FROM [dbo].[CustomerMarketingAttributes] AS [cma] WHERE [cma].[Id] = [cu].[CustomerMarketingAttributeId] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)) AS CustomerMarketingAttribute
	FROM [dbo].[Customers] AS [cu]
	FOR JSON PATH
) SELECT @JSON