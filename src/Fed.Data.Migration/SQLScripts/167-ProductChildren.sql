/****** Object:  Table [dbo].[ProductChildren]    Script Date: 19/03/2020 11:39:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ProductChildren](
	[ProductId] [NVARCHAR](200) NOT NULL,
	[ChildProductId] [NVARCHAR](200) NOT NULL,
 CONSTRAINT [PK_ProductChildren] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC,
	[ChildProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProductChildren]  WITH CHECK ADD  CONSTRAINT [FK_ProductChildren_ChildProducts] FOREIGN KEY([ChildProductId])
REFERENCES [dbo].[Products] ([Id])
GO

ALTER TABLE [dbo].[ProductChildren] CHECK CONSTRAINT [FK_ProductChildren_ChildProducts]
GO

ALTER TABLE [dbo].[ProductChildren]  WITH CHECK ADD  CONSTRAINT [FK_ProductChildren_ParentProducts] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO

ALTER TABLE [dbo].[ProductChildren] CHECK CONSTRAINT [FK_ProductChildren_ParentProducts]
GO