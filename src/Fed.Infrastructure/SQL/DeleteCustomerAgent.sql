UPDATE Customers SET CustomerAgentId = NULL WHERE CustomerAgentId = @Id


DELETE FROM [dbo].[CustomerAgents] WHERE Id = @Id