UPDATE [dbo].[Deliveries]
SET [PackingStatusId] = @PackingStatus
WHERE Id = @DeliveryId