ALTER TABLE [dbo].[Products] ADD [ProductCategoryId] UNIQUEIDENTIFIER NULL,
 FOREIGN KEY(ProductCategoryId) REFERENCES ProductCategories(Id);