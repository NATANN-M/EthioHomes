CREATE TABLE Messages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PropertyId INT NOT NULL,
    SenderId INT NOT NULL,
    ReceiverId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    SentDate DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (PropertyId) REFERENCES Properties(Id),
    FOREIGN KEY (SenderId) REFERENCES Users(Id),
    FOREIGN KEY (ReceiverId) REFERENCES Users(Id)
);
