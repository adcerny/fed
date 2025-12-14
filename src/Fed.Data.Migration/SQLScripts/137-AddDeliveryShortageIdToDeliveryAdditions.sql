SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS
(
    SELECT *
    FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[DeliveryAdditions]')
          AND [name] = 'DeliveryShortageId'
)
    ALTER TABLE [dbo].[DeliveryAdditions]
    ADD [DeliveryShortageId] UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS
(
    SELECT *
    FROM [sys].[foreign_keys]
    WHERE [object_id] = OBJECT_ID(N'dbo.FK_DeliveryAdditions_DeliveryShortages')
          AND [parent_object_id] = OBJECT_ID(N'dbo.DeliveryAdditions')
)
    ALTER TABLE [dbo].[DeliveryAdditions] WITH CHECK
    ADD CONSTRAINT [FK_DeliveryAdditions_DeliveryShortages]
        FOREIGN KEY ([DeliveryShortageId])
        REFERENCES [dbo].[DeliveryShortages] ([Id]);

GO