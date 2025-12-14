/****** Object:  Table [dbo].[ProductUnitConversions]    Script Date: 14/02/2019 13:14:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[Products] ADD [IsDeleted] [bit] NOT NULL
CONSTRAINT DV_Products_IsDeleted DEFAULT (0) WITH VALUES

GO
ALTER TABLE [dbo].[Products] ADD [DeletedDate] [datetime2](7) NULL