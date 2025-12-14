INSERT INTO [dbo].[Invoices] 
    ([Id] 
    ,[ContactId] 
    ,[FromDate] 
    ,[ToDate]
    ,[ExternalInvoiceNumber] 
    ,[ExternalInvoiceId] 
    ,[DateGenerated])
VALUES 
    (@Id 
    ,@ContactId 
    ,@FromDate 
    ,@ToDate 
    ,@ExternalInvoiceNumber 
    ,@ExternalInvoiceId 
    ,@DateGenerated )