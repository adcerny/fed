SELECT p.*,
       uc.[FedUnits],
       uc.[SupplierUnits],
       c.*,
       cuc.[FedUnits],
       cuc.[SupplierUnits]
FROM [dbo].[Products] AS [p]
  LEFT JOIN [dbo].[ProductUnitConversions] AS [uc]
    ON [p].[SupplierId] = [uc].[SupplierId]
       AND [p].[SupplierSKU] = [uc].[SupplierSKU]
  LEFT JOIN [dbo].[ProductChildren] AS [pc]
    ON [pc].[ProductId] = [p].[Id]
  LEFT JOIN [dbo].[Products] AS [c]
    ON [c].[Id] = [pc].[ChildProductId]
  LEFT JOIN [dbo].[ProductUnitConversions] AS [cuc]
    ON [c].[SupplierId] = [cuc].[SupplierId]
       AND [c].[SupplierSKU] = [cuc].[SupplierSKU]
WHERE [p].[Id] = @Id;