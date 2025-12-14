UPDATE [dbo].[Customers]
SET 
	[CompanyName] = @CompanyName, 
	[Website] = @Website, 
	[ACAccountNumber] = @ACAccountNumber, 
	[IsInvoiceable] = @IsInvoiceable, 
	[OfficeSizeMin] = @OfficeSizeMin, 
	[OfficeSizeMax] = @OfficeSizeMax, 
	[IsDeliveryChargeExempt] = @IsDeliveryChargeExempt, 
	[SplitDeliveriesByOrder] = @SplitDeliveriesByOrder,
	[IsTestAccount] = @IsTestAccount,
	[AccountTypeId] = @AccountTypeId,
	[Source] = @Source,
	[Notes] = @Notes,
	[IsFriend] = @IsFriend,
	[CancellationReason] = @CancellationReason,
	[CustomerAgentId] = @CustomerAgentId,
	[CustomerMarketingAttributeId] = @CustomerMarketingAttributeId
WHERE 
	Id = @Id