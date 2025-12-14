ALTER TABLE dbo.DeliveryBoundaryCoordinates  
DROP CONSTRAINT PK_DeliveryBoundaryCoordinates;   
GO  

ALTER TABLE dbo.DeliveryBoundaryCoordinates  
DROP COLUMN Id;   
GO  

ALTER TABLE dbo.DeliveryBoundaryCoordinates  ADD PRIMARY KEY(DeliveryBoundaryId,SortIndex);