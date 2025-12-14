SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DeliveryAdditionReasons]
(
	[Id] [int] NOT NULL,
	[Reason] [varchar](50) NOT NULL,
	CONSTRAINT [PK_DeliveryAdditionReasons] PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT INTO [dbo].[DeliveryAdditionReasons] ([Id], [Reason])
VALUES (0, N'Substitute')

CREATE TABLE [dbo].[DeliveryAdditions](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[ProductId] [nvarchar](200) NOT NULL,
	[Quantity] [int] NOT NULL,
	[Reason] [int] NOT NULL,
	[Notes] [varchar](max) NULL,
	CONSTRAINT [PK_DeliveryAdditions] PRIMARY KEY CLUSTERED ([Id] ASC) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[DeliveryAdditions] WITH CHECK ADD CONSTRAINT [FK_DeliveryAdditions_Orders] FOREIGN KEY ([OrderId])
REFERENCES [dbo].[Orders] ([Id])

ALTER TABLE [dbo].[DeliveryAdditions] WITH CHECK ADD CONSTRAINT [FK_DeliveryAdditions_Products] FOREIGN KEY ([ProductId])
REFERENCES [dbo].[Products] ([Id])

ALTER TABLE [dbo].[DeliveryAdditions] WITH CHECK ADD CONSTRAINT [FK_DeliveryAdditions_DeliveryAdditionReasons] FOREIGN KEY ([Reason])
REFERENCES [dbo].[DeliveryAdditionReasons] ([Id])

GO