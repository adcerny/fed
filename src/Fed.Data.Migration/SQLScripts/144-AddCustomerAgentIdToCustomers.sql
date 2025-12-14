ALTER TABLE [dbo].[Customers] ADD [CustomerAgentId] UNIQUEIDENTIFIER NULL,
 FOREIGN KEY(CustomerAgentId) REFERENCES CustomerAgents(Id);