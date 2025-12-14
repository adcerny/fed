SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Timeslots](
	[Id] [uniqueidentifier] NOT NULL,
	[HubId] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NOT NULL,
	[EarliestTime] [time](0) NOT NULL,
	[LatestTime] [time](0) NOT NULL,
	[TotalCapacity] [int] NOT NULL,
 CONSTRAINT [PK_Timeslots] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Timeslots] WITH CHECK ADD CONSTRAINT [FK_Timeslots_Hubs] FOREIGN KEY([HubId])
REFERENCES [dbo].[Hubs] ([Id])
GO

ALTER TABLE [dbo].[Timeslots] CHECK CONSTRAINT [FK_Timeslots_Hubs]
GO