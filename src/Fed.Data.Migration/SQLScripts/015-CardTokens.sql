SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CardTokens](
	[Id] [uniqueidentifier] NOT NULL,
	[ContactId] [uniqueidentifier] NOT NULL,
	[ExpiresYear] [int] NULL,
	[ExpiresMonth] [int] NULL,
	[ObscuredCardNumber] [varchar](20) NULL,
	[CardHolderFullName] [varchar](120) NULL,
	[AddressLine1] [varchar](250) NOT NULL,
	[Postcode] [varchar](10) NOT NULL,
	[IsPrimary] [bit] NOT NULL
 CONSTRAINT [PK_CardToken] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CardTokens]  WITH CHECK ADD  CONSTRAINT [FK_CardTokens_Contacts] FOREIGN KEY([ContactId])
REFERENCES [dbo].[Contacts] ([Id])
GO

ALTER TABLE [dbo].[CardTokens] CHECK CONSTRAINT [FK_CardTokens_Contacts]
GO