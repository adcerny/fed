DECLARE @Orders TABLE (Id UNIQUEIDENTIFIER)
INSERT INTO @Orders
SELECT Id FROM Orders WHERE [DeliveryDate] = @DeliveryDate

DECLARE @Deliveries TABLE (Id UNIQUEIDENTIFIER)
INSERT INTO @Deliveries
SELECT Id FROM Deliveries WHERE [DeliveryDate] = @DeliveryDate

DECLARE @Invoices TABLE (Id UNIQUEIDENTIFIER)
INSERT INTO @Invoices
SELECT InvoiceId FROM [dbo].[InvoiceDeliveries] 
INNER JOIN @Deliveries AS [d] ON [d].[Id] = [InvoiceDeliveries].[DeliveryId]

 DELETE FROM [dbo].[DeliveryOrders] WHERE [OrderId] In
 (SELECT Id FROM @Orders)

 DELETE FROM [dbo].[OrderProducts] WHERE [OrderId] IN
 (SELECT Id FROM @Orders)

  DELETE FROM [dbo].[OrderDiscounts] WHERE [OrderId] IN
 (SELECT Id FROM @Orders)

 DELETE FROM [dbo].[DeliveryAdditions] WHERE [OrderId] In
 (SELECT Id FROM @Orders)

  DELETE FROM [dbo].[DeliveryShortages] WHERE [OrderId] In
 (SELECT Id FROM @Orders)

 DELETE FROM [dbo].[Orders] WHERE [Id] IN
 (SELECT Id FROM @Orders)

 DELETE FROM [CardTransactions] WHERE [DeliveryId] IN 
  (SELECT Id FROM @Deliveries)

 DELETE FROM [InvoiceDeliveries] WHERE [InvoiceId] IN 
  (SELECT Id FROM @Invoices)

 DELETE FROM [Invoices] WHERE [Id] IN 
  (SELECT Id FROM @Invoices)

 DELETE FROM [dbo].[Deliveries] WHERE [Id] IN (SELECT Id FROM @Deliveries)