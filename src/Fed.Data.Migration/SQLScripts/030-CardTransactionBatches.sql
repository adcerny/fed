SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'payment')
BEGIN
	EXEC('CREATE SCHEMA payment')
END

CREATE TABLE [dbo].[CardTransactionBatches](
	[Id] [uniqueidentifier] NOT NULL,
	[DeliveryDate] [datetime] NOT NULL,
	[TimeStarted] [datetime] NOT NULL,
	[TimeEnded] [datetime] NULL
 CONSTRAINT [PK_CardTransactionBatches] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
GO