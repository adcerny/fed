SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CardTransactions](
	[Id] [uniqueidentifier] NOT NULL,
	[CardTransactionBatchId] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[CardTokenId] [uniqueidentifier] NOT NULL,
	[CardTransactionStatusId] [int] NOT NULL,
	[TimeCreated] [datetime] NOT NULL,
	[TimeModified] [datetime] NULL,
	[AmountRequested] [money] NULL,
	[AmountCaptured] [money] NULL,
	[ResponseCode] [varchar](1000) NULL,
	[ErrorMessage] [varchar](1000) NULL
 CONSTRAINT [PK_CardTransactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CardTransactions] WITH CHECK ADD CONSTRAINT [FK_CardTransactions_CardTransactionStatuses] FOREIGN KEY([CardTransactionStatusId])
REFERENCES [dbo].[CardTransactionStatuses] ([Id])
GO

ALTER TABLE [dbo].[CardTransactions] WITH CHECK ADD CONSTRAINT [FK_CardTransactions_Orders] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
GO

ALTER TABLE [dbo].[CardTransactions] WITH CHECK ADD CONSTRAINT [FK_CardTransactions_CardTransactionBatches] FOREIGN KEY([CardTransactionBatchId])
REFERENCES [dbo].[CardTransactionBatches] ([Id])
GO