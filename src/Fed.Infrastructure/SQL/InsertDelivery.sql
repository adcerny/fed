INSERT INTO [dbo].[Deliveries]
(
	[Id],
	[ShortId],
	[ContactId],
	[DeliveryAddressId],
	[DeliveryDate],
	[TimeslotId],
	[EarliestTime],
	[LatestTime],
	[DeliveryCharge],
	[DeliveryCompanyName],
	[DeliveryFullName],
	[DeliveryAddressLine1],
	[DeliveryAddressLine2],
	[DeliveryPostcode],
	[DeliveryTown],
	[DeliveryInstructions],
	[LeaveDeliveryOutside],
	[PackingStatusId],
	[BagCount]
)
VALUES
(
	@Id, 
	@ShortId, 
	@ContactId, 
	@DeliveryAddressId, 
	@DeliveryDate, 
	@TimeslotId, 
	@EarliestTime, 
	@LatestTime, 
	@DeliveryCharge,
	@DeliveryCompanyName, 
	@DeliveryFullName, 
	@DeliveryAddressLine1, 
	@DeliveryAddressLine2, 
	@DeliveryPostcode, 
	@DeliveryTown, 
	@DeliveryInstructions,
	@LeaveDeliveryOutside,
	@PackingStatusId,
	@BagCount
)

UPDATE [dbo].[Customers]
SET [FirstDeliveryDate] =
    (
        SELECT MIN([d].[DeliveryDate])
        FROM [dbo].[Deliveries] AS [d]
            INNER JOIN [dbo].[Contacts] AS [c]
                ON [c].[Id] = [d].[ContactId]
        WHERE [c].[CustomerId] = [Customers].[Id]
    )
WHERE [dbo].[Customers].[FirstDeliveryDate] IS NULL;