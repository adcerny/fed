INSERT INTO [dbo].[Orders] 
( 
    [Id], [ShortId], [RecurringOrderId], [OrderName], [DeliveryDate], [OrderGeneratedDate], [WeeklyRecurrence], 
    [TimeslotId], [EarliestTime], [LatestTime], 
    [HubId], [HubName], [HubPostcode], [HubAddressLine1], [HubAddressLine2], 
    [ContactId], [ContactShortId], [ContactFirstName], [ContactLastName], [ContactEmail], [ContactPhone], 
    [CustomerId], [CustomerShortId], [CompanyName],  
    [DeliveryAddressId], [DeliveryFullName], [DeliveryCompanyName], [DeliveryAddressLine1], [DeliveryAddressLine2], [DeliveryTown], [DeliveryPostcode], 
    [DeliveryInstructions], [LeaveDeliveryOutside], 
    [BillingAddressId], [BillingFullName], [BillingCompanyName], [BillingAddressLine1], [BillingAddressLine2], [BillingTown], [BillingPostcode], [BillingEmail], 
    [PaymentMethodId], [SplitDeliveriesByOrder], [IsFree] 
) 
SELECT 
    @Id, @ShortId, @RecurringOrderId, @OrderName, @DeliveryDate, GETDATE(), @WeeklyRecurrence,
    [ts].[Id], [ts].[EarliestTime], [ts].[LatestTime], 
    [hu].[Id], [hu].[Name], [hu].[Postcode], [hu].[AddressLine1], [hu].[AddressLine2], 
    [co].[Id], [co].[ShortId], [co].[FirstName], [co].[LastName], [co].[Email], [co].[Phone], 
    [cu].[Id], [cu].[ShortId], [cu].[CompanyName], 
    [da].[Id], [da].[FullName], [da].[CompanyName], [da].[AddressLine1], [da].[AddressLine2], [da].[Town], [da].[Postcode], 
    [da].[DeliveryInstructions], [da].[LeaveDeliveryOutside], 
    [ba].[Id], [ba].[FullName], [ba].[CompanyName], [ba].[AddressLine1], [ba].[AddressLine2], [ba].[Town], [ba].[Postcode], [ba].[Email], 
    [co].[PaymentMethodId], [cu].[SplitDeliveriesByOrder], @IsFree

  

  FROM [dbo].[Customers] AS [cu]
  INNER JOIN [dbo].[Contacts] AS [co] ON [co].[CustomerId] = [cu].[Id] 
  INNER JOIN [dbo].[DeliveryAddresses] AS [da] ON [da].[ContactId] = [co].[Id]
  INNER JOIN [dbo].[BillingAddresses] AS [ba] ON [ba].[ContactId] = [co].[Id]
  INNER JOIN [dbo].[Timeslots] AS [ts] ON [ts].[Id] = @TimeslotId
  INNER JOIN [dbo].[Hubs] AS [hu] ON [hu].[Id] = [ts].[HubId]
  WHERE [co].[Id] = @ContactId