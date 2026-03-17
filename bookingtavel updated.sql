CREATE TABLE [dbo].[Bookings] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [PropertyId] INT NOT NULL,
    [StartDate] DATE NOT NULL,        -- NEW
    [EndDate] DATE NOT NULL,          -- NEW
    [BookingDate] DATETIME DEFAULT (GETDATE()) NULL,
    [Status] NVARCHAR(50) DEFAULT ('Pending') NULL,
    [MessageToOwner] NVARCHAR(MAX) NULL,  -- NEW
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
    FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id])
);

