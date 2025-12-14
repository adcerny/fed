SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DeliveryAddresses](
	[Id] [uniqueidentifier] NOT NULL,
	[ContactId] [uniqueidentifier] NOT NULL,
	[IsPrimary] [bit] NOT NULL,
	[FullName] [varchar](250) NOT NULL,
	[CompanyName] [varchar](250) NULL,
	[AddressLine1] [varchar](250) NOT NULL,
	[AddressLine2] [varchar](250) NULL,
	[Town] [varchar](250) NOT NULL,
	[Postcode] [varchar](10) NOT NULL,
	[DeliveryInstructions] [varchar](max) NULL,
	[LeaveDeliveryOutside] [bit] NULL,
	[HubId] [uniqueidentifier] NOT NULL
 CONSTRAINT [PK_DeliveryAddresses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DeliveryAddresses] ADD DEFAULT ((0)) FOR [IsPrimary]
GO

ALTER TABLE [dbo].[DeliveryAddresses]  WITH CHECK ADD  CONSTRAINT [FK_DeliveryAddresses_Contacts] FOREIGN KEY([ContactId])
REFERENCES [dbo].[Contacts] ([Id])
GO

ALTER TABLE [dbo].[DeliveryAddresses] CHECK CONSTRAINT [FK_DeliveryAddresses_Contacts]
GO

ALTER TABLE [dbo].[DeliveryAddresses]  WITH CHECK ADD  CONSTRAINT [FK_DeliveryAddresses_Hubs] FOREIGN KEY([HubId])
REFERENCES [dbo].[Hubs] ([Id])
GO

ALTER TABLE [dbo].[DeliveryAddresses] CHECK CONSTRAINT [FK_DeliveryAddresses_Hubs]
GO