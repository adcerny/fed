IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'FirstDeliveryDate')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [FirstDeliveryDate] [DATE] NULL
END
GO
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