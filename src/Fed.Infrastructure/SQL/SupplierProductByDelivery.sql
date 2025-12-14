SELECT 
s.Id, 
s.[Name], 
p.ProductCode, 
c.CompanyName, 
op.Quantity, 
d.ShortId 
FROM [dbo].[Deliveries] AS [d]
INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [do].[DeliveryId] = [d].[Id]
INNER JOIN [dbo].[Orders] AS [o] ON [o].[Id] = [do].[OrderId]
INNER JOIN [dbo].[Contacts] AS [co] ON [co].[CustomerId] = [o].[CustomerId]
INNER JOIN [dbo].[Customers] AS [c] ON [c].[Id] = [co].[CustomerId]
INNER JOIN [dbo].[OrderProducts] AS [op] ON [op].[OrderId] = [o].[Id]
INNER JOIN [dbo].[Products] AS [p] ON [p].[Id] = [op].[ProductId]
INNER JOIN [dbo].[Suppliers] AS [s] ON [p].[SupplierId] = [s].[Id]
WHERE[d].[DeliveryDate] = CAST(@date AS DATETIME) 
group by s.Id, s.[Name], p.ProductCode, c.CompanyName, op.Quantity, d.ShortId
order by s.[Name], p.ProductCode, c.CompanyName, d.ShortId