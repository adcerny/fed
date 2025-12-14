UPDATE [dbo].[Discounts]
   SET [Id] = @Id
      ,[Name] = @Name
      ,[Description] = @Description
      ,[Percentage] = @Percentage
      ,[Value] = @Value
      ,[MinOrderValue] = @MinOrderValue
      ,[MaxOrderValue] = @MaxOrderValue
      ,[IsInactive] = @IsInactive
      ,[IsExclusive] = @IsExclusive
      ,[AppliedEventId] = @AppliedEventId
      ,[AppliedStartDate] = @AppliedStartDate
      ,[AppliedEndDate] = @AppliedEndDate
      ,[StartEventId] = @StartEventId
      ,[StartEventEndDate] = @StartEventEndDate
      ,[PeriodFromStartDays] = @PeriodFromStartDays
      ,[DiscountRewardTypeId] = @DiscountRewardTypeId
      ,[DiscountEligibleProductsTypeId] = @DiscountEligibleProductsTypeId
      ,[DiscountQualificationTypeId] = @DiscountQualificationTypeId
 WHERE Id = @Id

