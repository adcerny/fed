SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Discounts](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] NVARCHAR(250) NOT NULL,
	[Description] NVARCHAR(MAX) NOT NULL,
	[Code] NVARCHAR(250) NULL,
	[Percentage] [DECIMAL](8,2) NULL,
	[Value] [DECIMAL](18,2) NULL,
	[MinOrderValue] [DECIMAL](18,2) NULL,
	[MaxOrderValue] [DECIMAL](18,2) NULL,
	[IsInactive] BIT NOT NULL DEFAULT 0,
	[IsExclusive] BIT NOT NULL DEFAULT 0,
	[AppliedEventId] [INT] NOT NULL,
	[AppliedStartDate] DATE NOT NULL,
	[AppliedEndDate] DATE NULL,
	[StartEventId] [INT] NOT NULL,
	[StartEventEndDate] [DATE] NULL,
	[PeriodFromStartDays] [INT] NULL,
	CONSTRAINT [PK_Discounts] PRIMARY KEY CLUSTERED ([Id] ASC) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Discounts] WITH CHECK ADD CONSTRAINT [FK_DiscountsAppliedEvent_DiscountEvents] FOREIGN KEY ([AppliedEventId])
REFERENCES [dbo].[DiscountEvents] ([Id])

ALTER TABLE [dbo].[Discounts] WITH CHECK ADD CONSTRAINT [FK_DiscountsStartEvent_DiscountEvents] FOREIGN KEY ([StartEventId])
REFERENCES [dbo].[DiscountEvents] ([Id])

GO

INSERT [dbo].[Discounts]
(
    [Id],
    [Name],
    [Description],
    [Code],
    [Percentage],
    [Value],
    [MinOrderValue],
    [MaxOrderValue],
    [IsInactive],
    [IsExclusive],
    [AppliedEventId],
    [AppliedStartDate],
    [AppliedEndDate],
    [StartEventId],
    [StartEventEndDate],
    [PeriodFromStartDays]
)
VALUES
(N'690f40f7-153b-45b9-8641-39ef2df8d8ec', N'25% off first month', N'', N'', CAST(25.00 AS DECIMAL(8, 2)), NULL,
 CAST(20.00 AS DECIMAL(18, 2)), CAST(200.00 AS DECIMAL(18, 2)), 0, 0, 1, CAST(N'2019-09-05' AS DATE),
 CAST(N'2019-11-01' AS DATE), 2, CAST(N'2019-11-01' AS DATE), 30);
GO