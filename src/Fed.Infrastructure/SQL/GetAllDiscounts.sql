DECLARE @JSON nvarchar(MAX)
    =   (   
    SELECT [d].*,
       [d].[AppliedEventId] AS [AppliedEvent],
       [d].[StartEventId] AS [StartEvent],
       [d].[DiscountRewardTypeId] AS [RewardType],
       [d].[DiscountEligibleProductsTypeId] AS [EligibleProductsType],
       [d].[DiscountQualificationTypeId] AS [QualificationType],
       JSON_QUERY(
       (
         SELECT [dqc].[ProductCategoryId],
                [dqc].[ProductQuantity],
                JSON_QUERY(
                (
                  SELECT '[' + STUFF(
                               (
                                 SELECT ',' + '"' + CONVERT(VARCHAR(250), [p].[ProductCode]) + '"'
                                 FROM [dbo].[Products] AS [p]
                                 WHERE [p].[ProductCategoryId] = [dqc].[ProductCategoryId]
                                 FOR XML PATH('')
                               ),
                               1,
                               1,
                               ''
                                    ) + ']'
                )
                          ) AS [CategoryProductSkus]
         FROM [dbo].[DiscountQualifyingProductCategories] AS [dqc]
           INNER JOIN [dbo].[ProductCategories] AS [pc]
             ON [pc].[Id] = [dqc].[ProductCategoryId]
         WHERE [dqc].[DiscountId] = [d].[Id]
         FOR JSON PATH
       )
                 ) AS [QualifyingProductCategories],
       JSON_QUERY(
       (
         SELECT '[' + STUFF(
                      (
                        SELECT ',' + '"' + CONVERT(VARCHAR(250), [dec].[ProductCategoryId]) + '"'
                        FROM [dbo].[DiscountEligibleProductCategories] AS [dec]
                        WHERE [dec].[DiscountId] = [d].[Id]
                        FOR XML PATH('')
                      ),
                      1,
                      1,
                      ''
                           ) + ']'
       )
                 ) AS [EligibleProductCategoryIds],
       JSON_QUERY(
       (
         SELECT '[' + STUFF(
                      (
                        SELECT ',' + '"' + CONVERT(VARCHAR(250), [p].[ProductCode]) + '"'
                        FROM [dbo].[DiscountEligibleProductCategories] AS [dec]
                          INNER JOIN [dbo].[ProductCategories] AS [pc]
                            ON [pc].[Id] = [dec].[ProductCategoryId]
                          INNER JOIN [dbo].[Products] AS [p]
                            ON [p].[ProductCategoryId] = [pc].[Id] AND [p].[IsDeleted] = 0
                        WHERE [dec].[DiscountId] = [d].[Id]
                        FOR XML PATH('')
                      ),
                      1,
                      1,
                      ''
                           ) + ']'
       )
                 ) AS [EligibleProductCategorySkus],
       JSON_QUERY(
       (
         SELECT *
         FROM [dbo].[DiscountCodes]
         WHERE [DiscountId] = [d].[Id]
         FOR JSON PATH
       )
                 ) AS [DiscountCodes],
       JSON_QUERY(
       (
         SELECT [p].[ProductCode],
                [p].[ProductName],
                [dp].[Quantity],
                [dp].[Price]
         FROM [dbo].[DiscountedProducts] AS [dp]
           INNER JOIN [dbo].[Products] AS [p]
             ON [p].[Id] = [dp].[ProductId]
         WHERE [dp].[DiscountId] = [d].[Id]
         FOR JSON PATH
       )
                 ) AS [DiscountedProducts],
       JSON_QUERY(
       (
         SELECT [p].[ProductCode],
                [p].[ProductName],
                [dqp].[Quantity],
                0 AS [Price]
         FROM [dbo].[DiscountQualifyingProducts] AS [dqp]
           INNER JOIN [dbo].[Products] AS [p]
             ON [p].[Id] = [dqp].[ProductId]
         WHERE [dqp].[DiscountId] = [d].[Id]
         FOR JSON PATH
       )
                 ) AS [QualifyingProducts]
FROM [dbo].[Discounts] AS [d]
            FOR JSON PATH);

SELECT @JSON;