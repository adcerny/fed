CREATE TABLE [dbo].[ProductCategories](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ProductCategoryProducts](
	[ProductCategoryId] [uniqueidentifier] NOT NULL,
	[ProductId] [nvarchar](200) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProductCategoryId] ASC,
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProductCategoryProducts]  WITH CHECK ADD FOREIGN KEY([ProductCategoryId])
REFERENCES [dbo].[ProductCategories] ([Id])
GO

ALTER TABLE [dbo].[ProductCategoryProducts]  WITH CHECK ADD FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO

INSERT [dbo].[ProductCategories] ([Id], [Name]) VALUES (N'9289c23d-ff64-4419-84bc-0a76d6601667', N'Pastries')
GO
INSERT [dbo].[ProductCategoryProducts] ([ProductCategoryId], [ProductId]) VALUES (N'9289c23d-ff64-4419-84bc-0a76d6601667', N'2ff972b7-7051-4c92-8f25-2188ba6c547e')
GO
INSERT [dbo].[ProductCategoryProducts] ([ProductCategoryId], [ProductId]) VALUES (N'9289c23d-ff64-4419-84bc-0a76d6601667', N'46d591b0-a6b6-412d-a4ba-e16942891d4c')
GO
INSERT [dbo].[ProductCategoryProducts] ([ProductCategoryId], [ProductId]) VALUES (N'9289c23d-ff64-4419-84bc-0a76d6601667', N'492deb77-36b6-4e51-b08c-9ba67371a727')
GO
INSERT [dbo].[ProductCategoryProducts] ([ProductCategoryId], [ProductId]) VALUES (N'9289c23d-ff64-4419-84bc-0a76d6601667', N'755efb1c-860a-48fc-bb12-e664d02e33f7')
GO