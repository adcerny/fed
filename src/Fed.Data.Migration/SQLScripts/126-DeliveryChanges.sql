SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Deliveries]') AND [name] = 'PackingStatusId')
BEGIN
	CREATE TABLE [dbo].[PackingStatuses]
	(
		[Id] [int] NOT NULL,
		[PackingStatus] [nvarchar](50) NOT NULL,
		CONSTRAINT [PK_PackingStatuses] PRIMARY KEY CLUSTERED ([Id] ASC)
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	INSERT INTO [dbo].[PackingStatuses] ([Id], [PackingStatus])
	VALUES (0, N'Not Packed'), (1, N'Partially Packed'), (2, N'Packed')

    ALTER TABLE [dbo].[Deliveries] ADD [PackingStatusId] INT NOT NULL
	CONSTRAINT DV_Deliveries_PackingStatusId DEFAULT (0) WITH VALUES

	ALTER TABLE [dbo].[Deliveries] WITH CHECK ADD CONSTRAINT [FK_Deliveries_PackingStatuses] FOREIGN KEY ([PackingStatusId])
	REFERENCES [dbo].[PackingStatuses] ([Id])
END

IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Deliveries]') AND [name] = 'BagCount')
BEGIN
	ALTER TABLE [dbo].[Deliveries] ADD [BagCount] INT NOT NULL
	CONSTRAINT DV_Deliveries_BagCount DEFAULT (0) WITH VALUES
END

GO