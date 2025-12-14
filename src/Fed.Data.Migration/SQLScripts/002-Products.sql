SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Products](
	[Id] [nvarchar](200) NOT NULL,
	[ProductCode] [nvarchar](max) NOT NULL,
	[ProductGroup] [nvarchar](max) NULL,
	[ProductName] [nvarchar](max) NULL,
	[SupplierId] [nvarchar](1024) NULL,
	[SupplierSKU] [nvarchar](1024) NULL,
	[Price] [decimal](18, 2) NULL,
	[SalePrice] [decimal](18, 2) NULL,
	[IsTaxable] [BIT] NOT NULL
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO