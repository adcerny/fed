SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CustomerDiscounts](
	[CustomerId] [uniqueidentifier] NOT NULL,
	[DiscountId] [uniqueidentifier] NOT NULL,
	[AppliedDate] DATETIME NOT NULL,
	[EndDate] DATE NULL
	CONSTRAINT [PK_CustomerDiscounts] PRIMARY KEY CLUSTERED ([CustomerId], [DiscountId] ASC) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CustomerDiscounts] WITH CHECK ADD CONSTRAINT [FK_CustomerDiscounts_Customers] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO

ALTER TABLE [dbo].[CustomerDiscounts] WITH CHECK ADD CONSTRAINT [FK_CustomerDiscounts_Discount] FOREIGN KEY([DiscountId])
REFERENCES [dbo].[Discounts] ([Id])
GO