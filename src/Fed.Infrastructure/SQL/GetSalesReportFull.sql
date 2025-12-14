SELECT DISTINCT
	[cust].[ShortId] AS [CustomerId],
	[cust].[CompanyName],
	[del].[DeliveryDate],
	[del].[ShortId] AS [DeliveryId],
	[del].[DeliveryPostcode],
	[ord].[ShortId] AS [OrderId],
	[ord].[OrderName],
	[ordPro].[ProductCode],
	[ordPro].[ProductGroup],
	[ordPro].[ProductName],
	[ordPro].[SupplierId],
	[ordPro].[SupplierSKU],
	[ordPro].[Quantity],
	[ordPro].[Price],
	[ordPro].[SalePrice],
	(ISNULL([ordPro].[SalePrice], [ordPro].[Price]) * [ordPro].[Quantity]) AS [Total Price]
FROM [dbo].[Deliveries] AS [del]
INNER JOIN [dbo].[DeliveryOrders] AS [delOrd] ON [del].[Id] = [delOrd].[DeliveryId]
INNER JOIN [dbo].[Orders] AS [ord] ON [ord].[Id] = [delOrd].[OrderId]
INNER JOIN [dbo].[OrderProducts] AS [ordPro] ON [ord].[Id] = [ordPro].[OrderId]
INNER JOIN [dbo].[Contacts] AS [cont] ON [ord].[ContactId] = [cont].[Id]
INNER JOIN [dbo].[Customers] AS [cust] ON [cust].[Id] = [cont].[CustomerId]
WHERE
	[del].[DeliveryDate] >= @FromDate
	AND [del].[DeliveryDate] <= @ToDate