
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'customer')
BEGIN
	EXEC('CREATE SCHEMA customer')
END

CREATE TABLE [dbo].[Customers](
	[Id] [uniqueidentifier] NOT NULL,
	[ShortId] [nvarchar](50) NOT NULL,
	[CompanyName] [nvarchar](250) NOT NULL,
	[Website] [nvarchar](MAX) NULL,
	[ACAccountNumber] [int] NULL
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Customers] ADD CONSTRAINT [UQ_Customers_ShortId] UNIQUE ([ShortId])
GO