ALTER TABLE Bookings
ADD StartDate DATE NULL,
    EndDate DATE NULL,
    PaymentStatus NVARCHAR(50) DEFAULT 'NotPaid' NULL;
