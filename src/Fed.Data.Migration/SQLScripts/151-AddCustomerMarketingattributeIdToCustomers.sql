ALTER TABLE [dbo].[Customers] ADD [CustomerMarketingAttributeId] UNIQUEIDENTIFIER NULL,
 FOREIGN KEY(CustomerMarketingAttributeId) REFERENCES CustomerMarketingAttributes(Id);