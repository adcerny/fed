SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DeliveryShortages](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[ProductId] [nvarchar](200) NOT NULL,
	[DesiredQuantity] [int] NOT NULL,
	[ActualQuantity] [int] NOT NULL,
	[Reason] [varchar](max) NULL,
	CONSTRAINT [PK_DeliveryShortages] PRIMARY KEY CLUSTERED ([Id] ASC) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[DeliveryShortages] WITH CHECK ADD CONSTRAINT [FK_DeliveryShortages_Orders] FOREIGN KEY ([OrderId])
REFERENCES [dbo].[Orders] ([Id])

ALTER TABLE [dbo].[DeliveryShortages] WITH CHECK ADD CONSTRAINT [FK_DeliveryShortages_Products] FOREIGN KEY ([ProductId])
REFERENCES [dbo].[Products] ([Id])

GO