IF NOT EXISTS
(
    SELECT *
    FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[CardTransactions]')
          AND [name] = 'DeliveryId'
)
BEGIN
    ALTER TABLE [dbo].[CardTransactions]
    ADD [DeliveryId] UNIQUEIDENTIFIER NULL;
END;

GO

UPDATE [ct]
SET [ct].[DeliveryId] = [do].[DeliveryId]
FROM [dbo].[CardTransactions] [ct]
    INNER JOIN [dbo].[DeliveryOrders] AS [do]
        ON [do].[OrderId] = [ct].[OrderId];


GO

ALTER TABLE [dbo].[CardTransactions] WITH CHECK
ADD CONSTRAINT [FK_CardTransactions_Deliveries]
    FOREIGN KEY ([DeliveryId])
    REFERENCES [dbo].[Deliveries] ([Id]);
GO

ALTER TABLE [dbo].[CardTransactions] CHECK CONSTRAINT [FK_CardTransactions_Deliveries];
GO

ALTER TABLE [dbo].[CardTransactions] DROP CONSTRAINT [FK_CardTransactions_Orders]
GO

ALTER TABLE [dbo].[CardTransactions] DROP COLUMN [OrderId];
GO

