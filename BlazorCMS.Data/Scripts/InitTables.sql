-- Create Blog Table
CREATE TABLE IF NOT EXISTS BlogPosts (
    Id TEXT PRIMARY KEY,
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Create Page Table
CREATE TABLE IF NOT EXISTS Pages (
    Id TEXT PRIMARY KEY,
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Create Roles Table
CREATE TABLE IF NOT EXISTS AspNetRoles (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE
);

-- Create Users Table
CREATE TABLE IF NOT EXISTS AspNetUsers (
    Id TEXT PRIMARY KEY,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL
);

-- Insert Default Admin User
INSERT INTO AspNetUsers (Id, Email, PasswordHash) 
SELECT '1', 'admin@example.com', 'hashed-password'
WHERE NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email='admin@example.com');
