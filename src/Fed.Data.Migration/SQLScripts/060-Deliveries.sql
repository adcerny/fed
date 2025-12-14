SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Deliveries](
	[Id] [uniqueidentifier] NOT NULL,
	[ShortId] [nvarchar](50) NOT NULL,
	[ContactId] [uniqueidentifier] NOT NULL,
	[DeliveryAddressId] [uniqueidentifier] NOT NULL,
	[DeliveryDate] [date] NOT NULL,
	[TimeslotId] [uniqueidentifier] NOT NULL,
	[EarliestTime] [time](0) NOT NULL,
	[LatestTime] [time](0) NOT NULL,
	[DeliveryCompanyName] [varchar](250) NOT NULL,
	[DeliveryFullName] [varchar](250) NOT NULL,
	[DeliveryAddressLine1] [varchar](250) NOT NULL,
	[DeliveryAddressLine2] [varchar](250) NULL,
	[DeliveryPostcode] [varchar](10) NOT NULL,
	[DeliveryTown] [varchar](250) NOT NULL,
	[DeliveryInstructions] [varchar](max) NULL,
	[LeaveDeliveryOutside] [bit] NOT NULL,
	CONSTRAINT [PK_Deliveries] PRIMARY KEY CLUSTERED ([Id] ASC) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


CREATE TABLE [dbo].[DeliveryOrders]
(
	[DeliveryId] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	CONSTRAINT [PK_DeliveryOrders] PRIMARY KEY CLUSTERED 
	(
		[DeliveryId] ASC,
		[OrderId] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DeliveryOrders] WITH CHECK ADD CONSTRAINT [FK_DeliveryOrders_Deliveries] FOREIGN KEY ([DeliveryId])
REFERENCES [dbo].[Deliveries] ([Id])

ALTER TABLE [dbo].[DeliveryOrders] WITH CHECK ADD CONSTRAINT [FK_DeliveryOrders_Orders] FOREIGN KEY ([OrderId])
REFERENCES [dbo].[Orders] ([Id])
GO