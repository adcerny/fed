SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SkipDates](
	[RecurringOrderId] [uniqueidentifier] NOT NULL,
	[Date] [date] NOT NULL,
	[Reason] [nvarchar](max) NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[CreatedDateTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_SkipDates] PRIMARY KEY CLUSTERED 
(
	[RecurringOrderId] ASC,
	[Date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO