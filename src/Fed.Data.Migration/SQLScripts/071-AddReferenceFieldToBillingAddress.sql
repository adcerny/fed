IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[BillingAddresses]') AND [name] = 'InvoiceReference')
BEGIN
    ALTER TABLE [dbo].[BillingAddresses] ADD [InvoiceReference] NVARCHAR(250) NULL
END