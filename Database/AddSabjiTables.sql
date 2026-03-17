USE RoomExpenseTracker;
GO

CREATE TABLE SabjiTurns (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    UserId     NVARCHAR(50) NOT NULL,
    DayOfWeek  INT          NOT NULL,
    WeekOrder  INT          NOT NULL DEFAULT 0,
    CreatedAt  DATETIME2    NOT NULL DEFAULT GETDATE()
);

CREATE TABLE SabjiPurchases (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    UserId       NVARCHAR(50)  NOT NULL,
    UserName     NVARCHAR(100) NOT NULL,
    PurchaseDate DATETIME2     NOT NULL DEFAULT GETDATE(),
    Purchased    BIT           NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETDATE()
);

CREATE INDEX IX_SabjiTurns_DayOfWeek    ON SabjiTurns(DayOfWeek);
CREATE INDEX IX_SabjiPurchases_Date     ON SabjiPurchases(PurchaseDate);

PRINT 'Sabji tables created successfully!';
GO
