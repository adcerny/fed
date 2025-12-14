SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DiscountEvents](
	[Id] [INT] NOT NULL,
	[Name] [NVARCHAR](50) NOT NULL,
	[Description] [NVARCHAR](MAX) NULL,
 CONSTRAINT [PK_DiscountStartTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [dbo].[DiscountEvents]
(
    [Id],
    [Name],
    [Description]
)
VALUES
(   0,
    N'CodeEntered',
    N'A promotional code is entered by the user'
    ),
(   1,
    N'SignUp',
    N'A customer signs up via the website'
),
(   2,
    N'FirstOrder',
    N'A customer places their first order'
),
(   3,
    N'NextOrder',
    N'A customer places an order after the discount is applied'
);
GO