SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Orders](
	[Id] [uniqueidentifier] NOT NULL,
	[ShortId] [nvarchar](50) NOT NULL,
	[RecurringOrderId] [uniqueidentifier] NOT NULL,
	[OrderName] [nvarchar](max) NULL,
	[DeliveryDate] [date] NOT NULL,
	[OrderGeneratedDate] [datetime2] NOT NULL,
	
	[TimeslotId] [uniqueidentifier] NOT NULL,
	[EarliestTime] [time](0) NOT NULL,
	[LatestTime] [time](0) NOT NULL,

	[HubId] [uniqueidentifier] NOT NULL,
	[HubName] [nvarchar](50) NOT NULL,
	[HubPostcode] [nvarchar](50) NOT NULL,
	[HubAddressLine1] [nvarchar](255) NOT NULL,
	[HubAddressLine2] [nvarchar](255) NULL,

	[ContactId] [uniqueidentifier] NOT NULL,
	[ContactShortId] [nvarchar](50) NOT NULL,
	[ContactFirstName] [varchar](250) NOT NULL,
	[ContactLastName] [varchar](250) NOT NULL,
	[ContactEmail] [varchar](250) NOT NULL,
	[ContactPhone] [varchar](250) NULL,

	[CustomerId] [uniqueidentifier] NOT NULL,
	[CustomerShortId] [nvarchar](50) NOT NULL,
	[CompanyName] [varchar](250) NOT NULL,

	[DeliveryAddressId] [uniqueidentifier] NOT NULL,
	[DeliveryFullName] [varchar](250) NOT NULL,
	[DeliveryCompanyName] [varchar](250) NULL,
	[DeliveryAddressLine1] [varchar](250) NOT NULL,
	[DeliveryAddressLine2] [varchar](250) NULL,
	[DeliveryTown] [varchar](250) NOT NULL,
	[DeliveryPostcode] [varchar](10) NOT NULL,
	[DeliveryInstructions] [varchar](max) NULL,
	[LeaveDeliveryOutside] [bit] NULL,

	[PaymentMethodId] [int] NOT NULL,
	[CardTokenId] [uniqueidentifier] NULL,

	[BillingAddressId] [uniqueidentifier] NOT NULL,
	[BillingFullName] [varchar](250) NOT NULL,
	[BillingCompanyName] [varchar](250) NOT NULL,
	[BillingAddressLine1] [varchar](250) NOT NULL,
	[BillingAddressLine2] [varchar](250) NULL,
	[BillingTown] [varchar](250) NOT NULL,
	[BillingPostcode] [varchar](10) NOT NULL,
	[BillingEmail] [varchar](250) NOT NULL

 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_PaymentMethod] FOREIGN KEY([PaymentMethodId])
REFERENCES [dbo].[PaymentMethods] ([Id])
GO

ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_PaymentMethod]
GO

ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [UQ_Orders_ShortId] UNIQUE ([ShortId])
GO