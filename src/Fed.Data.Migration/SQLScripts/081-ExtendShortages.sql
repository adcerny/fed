SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[DeliveryShortages]') AND [name] = 'ReasonCode')
BEGIN
    ALTER TABLE [dbo].[DeliveryShortages] ADD [ReasonCode] NVARCHAR(50) NULL
END