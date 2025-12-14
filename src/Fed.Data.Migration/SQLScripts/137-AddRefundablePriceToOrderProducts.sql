SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS
(
    SELECT *
    FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[OrderProducts]')
          AND [name] = 'RefundablePrice'
)
    ALTER TABLE [dbo].[OrderProducts]
    ADD [RefundablePrice] [decimal](18, 2) NULL;
GO

UPDATE [dbo].[OrderProducts]
SET [RefundablePrice] = ISNULL([SalePrice], [Price])
WHERE [RefundablePrice] IS NULL;

GO

ALTER TABLE [dbo].[OrderProducts]
ALTER COLUMN [RefundablePrice] [DECIMAL](18, 2) NOT NULL;

GO