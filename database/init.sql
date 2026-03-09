IF DB_ID('JK_Configurations') IS NULL
BEGIN
    CREATE DATABASE JK_Configurations;
END
GO

USE JK_Configurations;
GO

IF OBJECT_ID('dbo.Configuration', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Configuration
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Key] NVARCHAR(200) NOT NULL,
        [Value] NVARCHAR(2000) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Configuration WHERE [Key] = 'HelloWorld')
BEGIN
    INSERT INTO dbo.Configuration (Id, [Key], [Value])
    VALUES (NEWID(), 'HelloWorld', 'Hello from DB');
END
GO