SELECT *
FROM [dbo].[SkipDates]
WHERE (
          [RecurringOrderId] = @RecurringOrderId
          OR @RecurringOrderId IS NULL
      )
      AND
      (
          [Date] >= @From
          OR @From IS NULL
      )
      AND
      (
          [Date] <= @To
          OR @To IS NULL
      )