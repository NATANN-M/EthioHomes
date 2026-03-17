CREATE TABLE [dbo].[Payments] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [BookingId] INT NOT NULL,
    [Amount] DECIMAL(10, 2) NOT NULL,
    [PaymentDate] DATETIME DEFAULT GETDATE(),
    [Status] NVARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY ([BookingId]) REFERENCES [dbo].[Bookings]([Id])
);
