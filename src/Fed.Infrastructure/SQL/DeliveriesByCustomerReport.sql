SELECT 
c.ShortId as [CustomerId],
c.CompanyName, 
a.AccountType,
d.DeliveryDate,
DATENAME(weekday,d.DeliveryDate) as [DayOfTheWeek],
DATENAME(week,d.DeliveryDate) as [WeekNumber],
d.ShortId as [DeliveryId],
o.DeliveryCompanyName,
o.DeliveryPostcode,
o.ShortId as [OrderId],
o.OrderName,
SUM(op.Quantity) as [ItemCountTotal],
SUM(op.Price) as [NetSalesTotal]
FROM [dbo].[Deliveries] AS [d]
INNER JOIN [dbo].[DeliveryOrders] AS [do] ON [do].[DeliveryId] = [d].[Id]
INNER JOIN [dbo].[Orders] AS [o] ON [o].[Id] = [do].[OrderId]
INNER JOIN [dbo].[OrderProducts] AS [op] ON [op].[OrderId] = [o].[Id]
INNER JOIN [dbo].[Contacts] AS [co] ON [co].[CustomerId] = [o].[CustomerId]
INNER JOIN [dbo].[Customers] AS [c] ON [c].[Id] = [co].[CustomerId] AND [c].[AccountTypeId] <> 1 --Exclude internal
INNER JOIN [dbo].[AccountTypes]  AS [a] ON [a].[Id] = [c].AccountTypeId
WHERE[d].[DeliveryDate] > CAST(@fromDate AS DATETIME) 
and [d].[DeliveryDate] < CAST(@toDate AS DATETIME) 
group by 
c.ShortId,
c.CompanyName, 
a.AccountType,
d.DeliveryDate,
DATENAME(weekday,d.DeliveryDate),
DATENAME(week,d.DeliveryDate),
d.ShortId ,
o.DeliveryCompanyName,
o.DeliveryPostcode,
o.ShortId,
o.OrderName
order by d.DeliveryDate