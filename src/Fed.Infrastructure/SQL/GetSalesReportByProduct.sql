SELECT
	[del].[DeliveryDate],
	[ordPro].[ProductGroup],
	[ordPro].[ProductCode],
	[ordPro].[ProductName],
	[ordPro].[SupplierId],
	[ordPro].[SupplierSKU],
	[ordPro].[Quantity],
	[ordPro].[Price],
	[ordPro].[SalePrice],
	SUM(ISNULL([ordPro].[SalePrice], [ordPro].[Price]) * [ordPro].[Quantity]) AS [Total Price]
FROM [dbo].[Deliveries] AS [del]
INNER JOIN [dbo].[DeliveryOrders] AS [delOrd] ON [del].[Id] = [delOrd].[DeliveryId]
INNER JOIN [dbo].[Orders] AS [ord] ON [ord].[Id] = [delOrd].[OrderId]
INNER JOIN [dbo].[OrderProducts] AS [ordPro] ON [ord].[Id] = [ordPro].[OrderId]
INNER JOIN [dbo].[Contacts] AS [cont] ON [ord].[ContactId] = [cont].[Id]
WHERE
	[del].[DeliveryDate] >= @FromDate
	AND [del].[DeliveryDate] <= @ToDate
GROUP BY
	[del].[DeliveryDate],
	[ordPro].[ProductGroup],
	[ordPro].[ProductCode],
	[ordPro].[ProductName],
	[ordPro].[SupplierId],
	[ordPro].[SupplierSKU],
	[ordPro].[Quantity],
	[ordPro].[Price],
	[ordPro].[SalePrice]
ORDER BY
	[ordPro].[ProductGroup],
	[del].[DeliveryDate],
	[ordPro].[ProductCode]