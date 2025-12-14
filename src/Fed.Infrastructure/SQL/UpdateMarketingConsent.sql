UPDATE [dbo].[Contacts]
SET [IsMarketingConsented] = @IsMarketingConsented
WHERE [Email] = @EmailAddress