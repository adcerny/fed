SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'IsTestAccount')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [IsTestAccount] BIT NOT NULL
	CONSTRAINT DV_Customers_IsTestAccount DEFAULT (0) WITH VALUES
END

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Customers]') AND [name] = 'AccountTypeId')
BEGIN

	CREATE TABLE [dbo].[AccountTypes]
	(
		[Id] [int] NOT NULL,
		[AccountType] [varchar](50) NOT NULL,
		CONSTRAINT [PK_AccountTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	INSERT INTO [dbo].[AccountTypes] ([Id], [AccountType])
	VALUES (0, N'Standard'), (1, N'Internal'), (2, N'Presale'), (3, N'Demo'), (4, N'Cancelled'), (5, N'Deleted')

	ALTER TABLE [dbo].[Customers] ADD [AccountTypeId] INT NOT NULL
	CONSTRAINT DV_Customers_AccountTypeId DEFAULT (0) WITH VALUES
END