DELETE FROM [dbo].[DeliveryAdditions] WHERE [DeliveryShortageId] = @Id

DELETE FROM [dbo].[DeliveryShortages] WHERE [Id] = @Id