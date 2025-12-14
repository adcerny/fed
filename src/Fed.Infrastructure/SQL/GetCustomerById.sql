DECLARE @JSON NVARCHAR(MAX) = (
	SELECT 
		* ,
		(SELECT MAX([DeliveryDate]) FROM [dbo].[Deliveries] AS [d]
		INNER JOIN [dbo].[Contacts] AS [co] ON [d].[ContactId] = [co].[Id]
		WHERE [co].[CustomerId] = @Id) AS [LastDeliveryDate],
		JSON_QUERY((
			SELECT 
				*, 
				JSON_QUERY((
					SELECT * FROM [dbo].[DeliveryAddresses] AS [da] WHERE [da].[ContactId] = [co].[Id] AND [da].[IsDeleted] = 0 FOR JSON PATH
				)) AS DeliveryAddresses, 
				JSON_QUERY((
					SELECT * FROM [dbo].[CardTokens] AS [ct] WHERE [ct].[ContactId] = [co].[Id] FOR JSON PATH
				)) AS CardTokens,
				JSON_QUERY((
					SELECT * FROM [dbo].[BillingAddresses] AS [ba] WHERE [ba].[ContactId] = [co].[Id] FOR JSON PATH
				)) AS BillingAddresses
			FROM [dbo].[Contacts] AS [co]
			WHERE [co].[CustomerId] = [cu].[Id] AND [co].[IsDeleted] = 0
			FOR JSON PATH
		)) AS Contacts,
		JSON_QUERY((
			SELECT * FROM [dbo].[CustomerAgents] AS [ca] WHERE [ca].[Id] = [cu].[CustomerAgentId] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
		)) AS CustomerAgent,
		JSON_QUERY((
			SELECT * FROM [dbo].[CustomerMarketingAttributes] AS [cma] WHERE [cma].[Id] = [cu].[CustomerMarketingAttributeId] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
		)) AS CustomerMarketingAttribute
	FROM [dbo].[Customers] AS [cu]
	WHERE [cu].[Id] = @Id
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
) SELECT @JSON