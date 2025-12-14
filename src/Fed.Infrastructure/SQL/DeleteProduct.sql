UPDATE [dbo].[Products]
SET [IsDeleted] = 1,
    [DeletedDate] = GETDATE()
WHERE [Id] = @ProductId