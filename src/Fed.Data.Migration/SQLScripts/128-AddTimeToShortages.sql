/****** Object:  Table [dbo].[ProductUnitConversions]    Script Date: 14/02/2019 13:14:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[DeliveryShortages] ADD [TimeRecorded] [time](0) NOT NULL
CONSTRAINT DV_DeliveryShortages_TimeRecorded DEFAULT (CONVERT(TIME, GETDATE())) WITH VALUES

GO
ALTER TABLE [dbo].[DeliveryAdditions] ADD [TimeRecorded] [time](0) NOT NULL
CONSTRAINT DV_DeliveryAdditions_TimeRecorded DEFAULT (CONVERT(TIME, GETDATE())) WITH VALUES