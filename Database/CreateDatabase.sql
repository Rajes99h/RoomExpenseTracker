-- ============================================================
--  AAJ KI Tarif — Room Expense Tracker
--  MS SQL Server Database Setup Script
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'RoomExpenseTracker')
    DROP DATABASE RoomExpenseTracker;
GO

CREATE DATABASE RoomExpenseTracker;
GO

USE RoomExpenseTracker;
GO

-- ============================================================
--  TABLES
-- ============================================================

CREATE TABLE Users (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      NVARCHAR(50)  NOT NULL UNIQUE,   -- login handle
    Name        NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role        NVARCHAR(20)  NOT NULL DEFAULT 'user',   -- 'user' | 'admin'
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETDATE(),
    IsActive    BIT           NOT NULL DEFAULT 1
);

CREATE TABLE Expenses (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      NVARCHAR(50)  NOT NULL,
    UserName    NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300) NOT NULL,
    Amount      DECIMAL(12,2) NOT NULL,
    Category    NVARCHAR(50)  NOT NULL,
    ExpenseDate DATE          NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Expenses_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE Absences (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    NVARCHAR(50) NOT NULL,
    FromDate  DATE         NOT NULL,
    ToDate    DATE         NOT NULL,
    CreatedAt DATETIME2    NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Absences_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- ============================================================
--  INDEXES
-- ============================================================
CREATE INDEX IX_Expenses_UserId      ON Expenses(UserId);
CREATE INDEX IX_Expenses_ExpenseDate ON Expenses(ExpenseDate);
CREATE INDEX IX_Absences_UserId      ON Absences(UserId);

-- ============================================================
--  SEED DATA  (admin password = "admin123", user = "pass123")
--  Passwords hashed with BCrypt — replace with actual hashes
--  from the app's first run or update via Admin panel
-- ============================================================

-- For demo the app stores BCrypt hashes; placeholders below.
-- Run the app once and register via /Admin or use the seed
-- method in ApplicationDbContext to auto-hash on startup.

INSERT INTO Users (UserId, Name, PasswordHash, Role) VALUES
('admin',  'Admin User',  'HASH_PLACEHOLDER_admin123',  'admin'),
('ravi',   'Ravi Kumar',  'HASH_PLACEHOLDER_pass123',   'user'),
('priya',  'Priya Sharma','HASH_PLACEHOLDER_pass123',   'user'),
('amit',   'Amit Singh',  'HASH_PLACEHOLDER_pass123',   'user');
GO

PRINT 'Database created successfully!';
GO

-- UserProfiles table
CREATE TABLE UserProfiles (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      NVARCHAR(50)  NOT NULL UNIQUE,
    AvatarEmoji NVARCHAR(10)  NOT NULL DEFAULT '👤',
    AvatarColor NVARCHAR(7)   NOT NULL DEFAULT '#a855f7',
    Bio         NVARCHAR(200) NULL,
    UpdatedAt   DATETIME2     NOT NULL DEFAULT GETDATE()
);
