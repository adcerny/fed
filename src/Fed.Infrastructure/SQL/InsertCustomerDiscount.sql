DECLARE @DiscountCodeId UNIQUEIDENTIFIER
SELECT @DiscountCodeId = Id FROM DiscountCodes WHERE Code = @DiscountCode

INSERT INTO [dbo].[CustomerDiscounts]
(
    [CustomerId],
    [DiscountId],
    [AppliedDate],
    [EndDate],
	[DiscountCodeId]
)
VALUES
(   
	@CustomerId,
    @DiscountId,
    @AppliedDate,
    @EndDate,
	@DiscountCodeId
)