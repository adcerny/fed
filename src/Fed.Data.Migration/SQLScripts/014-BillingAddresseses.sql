SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BillingAddresses](
	[Id] [uniqueidentifier] NOT NULL,
	[ContactId] [uniqueidentifier] NOT NULL,
	[IsPrimary] [bit] NOT NULL,
	[FullName] [varchar](250) NOT NULL,
	[CompanyName] [varchar](250) NOT NULL,
	[AddressLine1] [varchar](250) NOT NULL,
	[AddressLine2] [varchar](250) NULL,
	[Town] [varchar](250) NOT NULL,
	[Postcode] [varchar](10) NOT NULL,
	[Email] [nvarchar](250) NOT NULL,
	[Phone] [nvarchar](250) NULL,
 CONSTRAINT [PK_BillingAddresses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BillingAddresses] ADD DEFAULT ((0)) FOR [IsPrimary]
GO

ALTER TABLE [dbo].[BillingAddresses]  WITH CHECK ADD CONSTRAINT [FK_BillingAddresses_Contacts] FOREIGN KEY([ContactId])
REFERENCES [dbo].[Contacts] ([Id])
GO

ALTER TABLE [dbo].[BillingAddresses] CHECK CONSTRAINT [FK_BillingAddresses_Contacts]
GO