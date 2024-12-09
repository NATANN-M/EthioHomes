CREATE TABLE Properties (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(100) NOT NULL,
    Location NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    PropertyType NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Bedrooms INT NOT NULL,
    Bathrooms INT NOT NULL,
    Description NVARCHAR(MAX),
    FOREIGN KEY (UserId) REFERENCES Users(Id) 
);
