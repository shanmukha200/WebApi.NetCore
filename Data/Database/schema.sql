IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        Email NVARCHAR(200) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(512) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL
    );
END
GO

IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Token NVARCHAR(256) NOT NULL UNIQUE,
        ExpiryDate DATETIME2 NOT NULL,
        IsRevoked BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens(UserId);
END
GO
