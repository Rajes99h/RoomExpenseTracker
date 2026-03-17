USE RoomExpenseTracker;
GO
CREATE TABLE DustTurns (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    NVARCHAR(50) NOT NULL,
    DayOfWeek INT NOT NULL,
    WeekOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
CREATE TABLE DustPurchases (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    NVARCHAR(50)  NOT NULL,
    UserName  NVARCHAR(100) NOT NULL,
    ThrowDate DATETIME2     NOT NULL DEFAULT GETDATE(),
    Thrown    BIT           NOT NULL DEFAULT 1,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETDATE()
);
GO
PRINT 'Dust tables created!';
