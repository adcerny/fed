SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Suppliers](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [dbo].[Suppliers]
(
    [Id],
    [Name]
)
VALUES
(   7, 
	N'Seven Seeded'
),
(   8, 
	N'Abel & Cole'
),
(   209,  -- Id - int
	N'Supernote Ltd T/A Yummy Tummy'
),
(
291,
'Planet Organic'
)
GO