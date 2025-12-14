UPDATE [dbo].[Products]
SET [ProductCode] = @ProductCode,
    [ProductGroup] = @ProductGroup,
    [ProductName] = @ProductName,
    [SupplierId] = @SupplierId,
    [SupplierSKU] = @SupplierSKU,
    [Price] = @Price,
    [SalePrice] = @SalePrice,
    [IsTaxable] = @IsTaxable,
    [IconCategory] = @IconCategory,
    [IsDeleted] = 0,
    [DeletedDate] = NULL,
    [ProductCategoryId] = @ProductCategoryId,
    [IsShippable] = @IsShippable
WHERE [Id] = @Id