DECLARE @JSON [NVARCHAR](MAX) =
        (
			SELECT [i].*,
                   JSON_QUERY(
                   (
                       SELECT
                           *,
                           -- <Get all orders for each delivery> --
                           JSON_QUERY(
                           (
                               SELECT [o2].*,
                                      JSON_QUERY(
                                      (
                                          SELECT *
                                          FROM [dbo].[OrderProducts]
                                          WHERE [OrderId] = [o2].[Id]
                                          FOR JSON PATH
                                      )
                                                ) AS [OrderItems],
                                      JSON_QUERY(
                                      (
                                          SELECT *
                                          FROM [dbo].[OrderDiscounts]
                                          WHERE [OrderId] = [o2].[Id]
                                          FOR JSON PATH
                                      )
                                                ) AS [OrderDiscounts]
                               FROM [dbo].[Orders] AS [o2]
                                   INNER JOIN [dbo].[DeliveryOrders] AS [do]
                                       ON [do].[OrderId] = [o2].[Id]
                               WHERE [do].[DeliveryId] = [d].[Id]
                               FOR JSON PATH
                           )
                                     ) AS [Orders]
                       -- </ Get all orders for each delivery> --

                       FROM [dbo].[Deliveries] AS [d]
                           INNER JOIN [dbo].[InvoiceDeliveries] [id2]
                               ON [id2].[DeliveryId] = [d].[Id]
                                  AND [id2].[InvoiceId] = [i].[Id]
                       FOR JSON PATH
                   )
                             ) AS [Deliveries]
            FROM [dbo].[Invoices] [i]
                INNER JOIN [dbo].[InvoiceDeliveries] [id]
                    ON [id].[InvoiceId] = [i].[Id]
            WHERE [i].[ContactId] = @ContactId
            GROUP BY [i].[Id],
                     [i].[ContactId],
                     [i].[FromDate],
                     [i].[ToDate],
                     [i].[ExternalInvoiceNumber],
                     [i].[ExternalInvoiceId],
                     [i].[DateGenerated]
            FOR JSON PATH
        );
SELECT @JSON;