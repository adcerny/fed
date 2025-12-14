
IF NOT EXISTS
(
    SELECT *
    FROM [CardTransactionStatuses]
    WHERE [Name] = 'Refunded'
)
BEGIN

    INSERT INTO [dbo].[CardTransactionStatuses]
    (
        [Name],
        [Desc]
    )
    VALUES
    (   'Refunded',                -- Name - varchar(50)
        'Amount has been refunded' -- Desc - varchar(max)
        );

END;