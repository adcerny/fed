/****** Object:  Table [dbo].[DeliveryBoundaries]    Script Date: 18/12/2018 14:27:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DeliveryBoundaries](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] varchar(250) NOT NULL,
	[HubId] [uniqueidentifier] NOT NULL
 CONSTRAINT [PK_DeliveryBoundaries] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DeliveryBoundaries] WITH CHECK ADD CONSTRAINT [FK_DeliveryBoundaries_Hubs] FOREIGN KEY([HubId])
REFERENCES [dbo].[Hubs] ([Id])
GO

ALTER TABLE [dbo].[DeliveryBoundaries] CHECK CONSTRAINT [FK_DeliveryBoundaries_Hubs]
GO

CREATE TABLE [dbo].[DeliveryBoundaryCoordinates](
	[Id] [uniqueidentifier] NOT NULL,
	[DeliveryBoundaryId] [uniqueidentifier] NOT NULL,
	[SortIndex] [int] NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL
 CONSTRAINT [PK_DeliveryBoundaryCoordinates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


ALTER TABLE [dbo].[DeliveryBoundaryCoordinates] WITH CHECK ADD CONSTRAINT [FK_DeliveryBoundaryCoordinates_DeliveryBoundaries] FOREIGN KEY([DeliveryBoundaryId])
REFERENCES [dbo].[DeliveryBoundaries] ([Id])
GO

ALTER TABLE [dbo].[DeliveryBoundaryCoordinates] CHECK CONSTRAINT [FK_DeliveryBoundaryCoordinates_DeliveryBoundaries]
GO
