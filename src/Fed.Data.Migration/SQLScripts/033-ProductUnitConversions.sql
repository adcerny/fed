/****** Object:  Table [dbo].[ProductUnitConversions]    Script Date: 14/02/2019 13:14:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ProductUnitConversions](
	[SupplierId] [varchar](1024) NOT NULL,
	[SupplierSKU] [varchar](1024) NOT NULL,
	[FedUnits] [int] NOT NULL,
	[SupplierUnits] [int] NOT NULL,
 CONSTRAINT [UQ_ProductUnitConversions] UNIQUE NONCLUSTERED 
(
	[SupplierId] ASC,
	[SupplierSKU] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



