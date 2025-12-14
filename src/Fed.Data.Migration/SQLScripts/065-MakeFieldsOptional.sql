SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Update DeliveryAddresses table to allow more nulls and unicode characters

ALTER TABLE [dbo].[DeliveryAddresses] ALTER COLUMN [FullName] [nvarchar](250) NULL
ALTER TABLE [dbo].[DeliveryAddresses] ALTER COLUMN [CompanyName] [nvarchar](250) NULL
ALTER TABLE [dbo].[DeliveryAddresses] ALTER COLUMN [AddressLine1] [nvarchar](250) NULL
ALTER TABLE [dbo].[DeliveryAddresses] ALTER COLUMN [AddressLine2] [nvarchar](250) NULL
ALTER TABLE [dbo].[DeliveryAddresses] ALTER COLUMN [Town] [nvarchar](250) NULL
ALTER TABLE [dbo].[DeliveryAddresses] ALTER COLUMN [Postcode] [nvarchar](10) NULL
ALTER TABLE [dbo].[DeliveryAddresses] ALTER COLUMN [DeliveryInstructions] [nvarchar](MAX) NULL

-- Update Orders table to allow more nulls

ALTER TABLE [dbo].[Orders] ALTER COLUMN [HubName] [nvarchar](50) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [HubPostcode] [nvarchar](50) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [HubAddressLine1] [nvarchar](255) NULL

ALTER TABLE [dbo].[Orders] ALTER COLUMN [ContactFirstName] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [ContactLastName] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [ContactEmail] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CompanyName] [nvarchar](250) NULL

ALTER TABLE [dbo].[Orders] ALTER COLUMN [DeliveryFullName] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [DeliveryCompanyName] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [DeliveryAddressLine1] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [DeliveryAddressLine2] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [DeliveryTown] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [DeliveryPostcode] [nvarchar](10) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [DeliveryInstructions] [nvarchar](MAX) NULL

ALTER TABLE [dbo].[Orders] ALTER COLUMN [BillingFullName] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [BillingCompanyName] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [BillingAddressLine1] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [BillingAddressLine2] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [BillingTown] [nvarchar](250) NULL
ALTER TABLE [dbo].[Orders] ALTER COLUMN [BillingPostcode] [nvarchar](10) NULL

-- Update Deliveries table to allow more nulls

ALTER TABLE [dbo].[Deliveries] ALTER COLUMN [DeliveryCompanyName] [nvarchar](250) NULL
ALTER TABLE [dbo].[Deliveries] ALTER COLUMN [DeliveryFullName] [nvarchar](250) NULL
ALTER TABLE [dbo].[Deliveries] ALTER COLUMN [DeliveryAddressLine1] [nvarchar](250) NULL
ALTER TABLE [dbo].[Deliveries] ALTER COLUMN [DeliveryAddressLine2] [nvarchar](250) NULL
ALTER TABLE [dbo].[Deliveries] ALTER COLUMN [DeliveryPostcode] [nvarchar](10) NULL
ALTER TABLE [dbo].[Deliveries] ALTER COLUMN [DeliveryTown] [nvarchar](250) NULL
ALTER TABLE [dbo].[Deliveries] ALTER COLUMN [DeliveryInstructions] [nvarchar](MAX) NULL