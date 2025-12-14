SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DiscountRewardType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DiscountRewardType] [varchar](250) NOT NULL,
 CONSTRAINT [PK_DiscountRewardType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT INTO [dbo].[DiscountRewardType]
(
  [DiscountRewardType]
)
VALUES
('Percentage'),
('Value'),
('Product')

CREATE TABLE [dbo].[DiscountEligibleProductsType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DiscountEligibleProductsType] [varchar](250) NOT NULL,
 CONSTRAINT [PK_DiscountEligibleProductsType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT INTO [dbo].[DiscountEligibleProductsType]
(
  [DiscountEligibleProductsType]
)
VALUES
('AllProducts'),
('Category')

CREATE TABLE [dbo].[DiscountQualificationType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DiscountQualificationType] [varchar](250) NOT NULL,
 CONSTRAINT [PK_DiscountQualificationType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT INTO [dbo].[DiscountQualificationType]
(
  [DiscountQualificationType]
)
VALUES
('MinimumOrderValue'),
('CategorySpend'),
('ProductPurchase')