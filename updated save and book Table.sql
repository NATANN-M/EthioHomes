CREATE TABLE SavedProperties (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    PropertyId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (PropertyId) REFERENCES Properties(Id)
);

CREATE TABLE Bookings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    PropertyId INT NOT NULL,
    BookingDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (PropertyId) REFERENCES Properties(Id)
);
