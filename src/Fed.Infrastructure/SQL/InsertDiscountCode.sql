IF NOT EXISTS (SELECT [Id] FROM [dbo].[DiscountCodes] WHERE Code = @Code)

INSERT INTO [dbo].[DiscountCodes]
(
    [Id],
    [DiscountId],
    [Code],
    [Description],
    [IsInactive]
)
VALUES
(   @Id,
    @DiscountId,
    @Code,
    @Description,
    @IsInactive
    );