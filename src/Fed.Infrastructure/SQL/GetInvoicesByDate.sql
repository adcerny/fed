DECLARE @JSON [nvarchar](MAX) =
(
    SELECT 
		'00000000-0000-0000-0000-000000000000'	AS [Id],
        [c].[Id]								AS [ContactId],
        @From									AS [FromDate],
        @To										AS [ToDate],
        [ba].[InvoiceReference]					AS [Reference],

		-- <All deliveries for each contact>
        JSON_QUERY ((
            SELECT
				-- <Get all delivery details>
				*,
				-- </Get all delivery details>

				-- <Get all orders for each delivery> --
                JSON_QUERY ((
                    SELECT
						[o2].*,
                        JSON_QUERY ((
                            SELECT *
                            FROM [dbo].[OrderProducts]
                            WHERE [OrderId] = [o2].[Id]
                            FOR JSON PATH
                        )) AS [OrderItems],
						 JSON_QUERY((
                            SELECT *
                            FROM [dbo].[OrderDiscounts]
                            WHERE [OrderId] = [o2].[Id]
                            FOR JSON PATH
                        )) AS [OrderDiscounts]
					FROM [dbo].[Orders]					AS [o2]
					INNER JOIN [dbo].[DeliveryOrders]	AS [do] ON [do].[OrderId] = [o2].[Id]
					WHERE [do].[DeliveryId] = [d].[Id]
					FOR JSON PATH
                )) AS [Orders],
				-- </ Get all orders for each delivery> --

				-- <Get all shortages for each delivery> --
                JSON_QUERY ((
					SELECT 
						[ds].*,
						[prd].[ProductName],

						-- <Get all replacements for each shortage> --
						JSON_QUERY((
							SELECT
								[da].*,
								[prd].[ProductName],
								[prd].[ProductCode],
								ISNULL([prd].[SalePrice], [prd].[Price]) AS [ProductPrice],
								[prd].[IsTaxable]
							FROM [dbo].[DeliveryAdditions] AS [da]
							INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [da].[OrderId] = [do].[OrderId]
							LEFT JOIN [dbo].[Products] AS [prd] ON [prd].[Id] = [da].[ProductId]
							WHERE [da].[DeliveryShortageId] = [ds].[Id]
							FOR JSON PATH
						)) AS [Replacements]
						-- </Get all replacements for each shortage> --

					FROM [dbo].[DeliveryShortages]		AS [ds]
					INNER JOIN [dbo].[DeliveryOrders]	AS [do2] ON [ds].[OrderId] = [do2].[OrderId]
					LEFT JOIN [dbo].[Products]			AS [prd] ON [prd].[Id] = [ds].[ProductId]
					WHERE [do2].[DeliveryId] = [d].[Id]
					FOR JSON PATH
                )) AS [DeliveryShortages]
				-- </Get all shortages for each delivery> --

				

            FROM [dbo].[Deliveries] AS [d]
            WHERE 
				[d].[ContactId] = [c].[Id] 
				AND [d].[DeliveryDate] >= @From 
				AND [d].[DeliveryDate] <= @To
            FOR JSON PATH
        )) AS [Deliveries],
		-- </All deliveries for each contact>

		-- <All payments for each contact>
        JSON_QUERY	((
            SELECT [ct].*
            FROM [dbo].[CardTransactions] AS [ct]
            INNER JOIN [dbo].[Deliveries] AS [d2]	ON  [d2].[ContactId] = [c].[Id]
													AND [d2].[Id] = [ct].[DeliveryId]
													AND [ct].[CardTransactionStatusId] = 2
													AND [d2].[DeliveryDate] >= @From
													AND [d2].[DeliveryDate] <= @To
            FOR JSON PATH
        )) AS [Payments],
		-- </All payments for each contact>

		-- <All refunds for each contact>
        JSON_QUERY ((
            SELECT [ct].*
            FROM [dbo].[CardTransactions] AS [ct]
            INNER JOIN [dbo].[Deliveries] AS [d2]	ON  [d2].[ContactId] = [c].[Id]
													AND [d2].[Id] = [ct].[DeliveryId]
													AND [ct].[CardTransactionStatusId] = 4
													AND [d2].[DeliveryDate] >= @From
													AND [d2].[DeliveryDate] <= @To
            FOR JSON PATH
        )) AS [Refunds]
		-- </All refunds for each contact>

    FROM [dbo].[Contacts]					AS [c]
    INNER JOIN [dbo].[Deliveries]			AS [d3]		ON  [d3].[ContactId] = [c].[Id]
														AND [d3].[DeliveryDate] >= @From	-- Only generate invoices for deliveries within
														AND [d3].[DeliveryDate] <= @To		-- the given date range
    LEFT JOIN  [dbo].[InvoiceDeliveries]	AS [id]		ON  [id].[DeliveryId] = [d3].[Id]
    INNER JOIN [dbo].[Customers]			AS [cu]		ON  [cu].[Id] = [c].[CustomerId]
														AND [cu].[IsInvoiceable] = 1		-- Customer must be flagged as invoicable
														AND [cu].[AccountTypeId] <> 1		-- Customer must not be internal (Internals don't get invoiced)
														AND [cu].[AccountTypeId] <> 2		-- Customer must not be Presale (Presale customers don't get invoiced)
														AND [cu].[AccountTypeId] <> 3		-- Customer must not be Demo (Demo customer's orders never get processed)
    INNER JOIN [dbo].[BillingAddresses]		AS [ba]		ON  [ba].[ContactId] = [c].[Id]
														AND [ba].[IsPrimary] = 1			-- Generate invoices for the primary billing contacts only
    WHERE [id].[InvoiceId] IS NULL
    GROUP BY 
		[c].[Id],
        [ba].[InvoiceReference]
    FOR JSON PATH
);
SELECT @JSON;