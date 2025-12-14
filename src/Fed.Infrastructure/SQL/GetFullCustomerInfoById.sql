DECLARE @JSON NVARCHAR(MAX) = (
	SELECT 
		-- Customer Details --
		* ,
		-- Deliveries (incl. Orders and OrderItems) --
		JSON_QUERY((
			SELECT
				[d].*,
				JSON_QUERY((
					SELECT 
						[od].*,
						JSON_QUERY((
							SELECT * FROM [dbo].[OrderProducts]
							WHERE [OrderId] = [od].[Id]
							FOR JSON PATH
						)) AS [OrderItems],
						JSON_QUERY((
							SELECT *
							FROM [dbo].[OrderDiscounts]
							WHERE [OrderId] = [od].[Id]
							FOR JSON PATH
						)) AS [OrderDiscounts]
					FROM [dbo].[Orders] AS [od]
					INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [do].[OrderId] = [od].[Id]
					WHERE [do].[DeliveryId] = [d].[Id]
					FOR JSON PATH
				)) AS [Orders]
			FROM [dbo].[Deliveries] AS [d]
			INNER JOIN [dbo].[Contacts] AS [co] ON [d].[ContactId] = [co].[Id]
			WHERE [co].[CustomerId] = [cu].[Id]
			FOR JSON PATH
		)) AS [Deliveries],
		-- Last Delivery Date --
		(SELECT MAX([DeliveryDate]) FROM [dbo].[Deliveries] AS [d]
		INNER JOIN [dbo].[Contacts] AS [co] ON [d].[ContactId] = [co].[Id]
		WHERE [co].[CustomerId] = [cu].[Id]) AS [LastDeliveryDate],
		-- Contacts (incl. DeliveryAddresses, BillingAddresses and CardTokens) --
		JSON_QUERY((
			SELECT 
				*, 
				JSON_QUERY((
					SELECT * FROM [dbo].[DeliveryAddresses] AS [da] WHERE [da].[ContactId] = [co].[Id] AND [da].[IsDeleted] = 0 FOR JSON PATH
				)) AS [DeliveryAddresses], 
				JSON_QUERY((
					SELECT * FROM [dbo].[CardTokens] AS [ct] WHERE [ct].[ContactId] = [co].[Id] FOR JSON PATH
				)) AS [CardTokens],
				JSON_QUERY((
					SELECT * FROM [dbo].[BillingAddresses] AS [ba] WHERE [ba].[ContactId] = [co].[Id] FOR JSON PATH
				)) AS [BillingAddresses]
			FROM [dbo].[Contacts] AS [co]
			WHERE [co].[CustomerId] = [cu].[Id] AND [co].[IsDeleted] = 0
			FOR JSON PATH
		)) AS [Contacts],
		JSON_QUERY((
			SELECT * FROM [dbo].[CustomerAgents] AS [ca] WHERE [ca].[Id] = [cu].[CustomerAgentId] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
		)) AS CustomerAgent,
		JSON_QUERY((
			SELECT * FROM [dbo].[CustomerMarketingAttributes] AS [cma] WHERE [cma].[Id] = [cu].[CustomerMarketingAttributeId] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
		)) AS CustomerMarketingAttribute
	FROM [dbo].[Customers] AS [cu]
	WHERE CAST(([cu].[Id]) AS [nvarchar](36)) = @Id OR [cu].[ShortId] = @Id
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
) SELECT @JSON