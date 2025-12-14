IF NOT EXISTS
(
    SELECT *
    FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Timeslots]')
          AND [name] = 'IsActive'
)


ALTER TABLE [dbo].[Timeslots]
ADD [IsActive] BIT NOT NULL DEFAULT 1

GO