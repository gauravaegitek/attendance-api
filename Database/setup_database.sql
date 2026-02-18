-- -- =============================================
-- -- Attendance Management System - Database Setup
-- -- =============================================

-- USE master;
-- GO

-- -- Create Database
-- IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AttendanceDB')
-- BEGIN
--     CREATE DATABASE AttendanceDB;
-- END
-- GO

-- USE AttendanceDB;
-- GO

-- -- =============================================
-- -- Users Table
-- -- =============================================
-- IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'users')
-- BEGIN
--     CREATE TABLE users (
--         userid INT IDENTITY(1,1) PRIMARY KEY,
--         username NVARCHAR(100) NOT NULL,
--         email NVARCHAR(100) NOT NULL UNIQUE,
--         passwordhash NVARCHAR(255) NOT NULL,
--         role NVARCHAR(50) NOT NULL,
--         deviceid NVARCHAR(255) NULL,
--         lastseen DATETIME NULL,
--         createdon DATETIME NOT NULL DEFAULT GETDATE(),
--         isactive BIT NOT NULL DEFAULT 1
--     );

--     CREATE INDEX IX_users_email ON users(email);
--     CREATE INDEX IX_users_role ON users(role);
-- END
-- GO

-- -- =============================================
-- -- Attendance Table
-- -- =============================================
-- IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'attendance')
-- BEGIN
--     CREATE TABLE attendance (
--         attendanceid INT IDENTITY(1,1) PRIMARY KEY,
--         userid INT NOT NULL,
--         attendancedate DATE NOT NULL,
        
--         -- Check-In Details
--         intime TIME NULL,
--         intimedatetime DATETIME NULL,
--         inlatitude DECIMAL(10,7) NULL,
--         inlongitude DECIMAL(10,7) NULL,
--         inlocationaddress NVARCHAR(500) NULL,
--         inselfie NVARCHAR(500) NULL,
        
--         -- Check-Out Details
--         outtime TIME NULL,
--         outtimedatetime DATETIME NULL,
--         outlatitude DECIMAL(10,7) NULL,
--         outlongitude DECIMAL(10,7) NULL,
--         outlocationaddress NVARCHAR(500) NULL,
--         outselfie NVARCHAR(500) NULL,
        
--         -- Calculated Fields
--         totalhours DECIMAL(5,2) NULL,
        
--         -- Audit Fields
--         createdon DATETIME NOT NULL DEFAULT GETDATE(),
--         updatedon DATETIME NULL,
        
--         -- Foreign Key
--         CONSTRAINT FK_attendance_users FOREIGN KEY (userid) 
--             REFERENCES users(userid) ON DELETE CASCADE,
        
--         -- Unique Constraint - One attendance per user per day
--         CONSTRAINT UQ_attendance_user_date UNIQUE(userid, attendancedate)
--     );

--     CREATE INDEX IX_attendance_userid ON attendance(userid);
--     CREATE INDEX IX_attendance_date ON attendance(attendancedate);
-- END
-- GO

-- -- =============================================
-- -- Insert Default Admin User
-- -- Password: Admin@123 (BCrypt hashed)
-- -- =============================================
-- IF NOT EXISTS (SELECT * FROM users WHERE email = 'admin@attendance.com')
-- BEGIN
--     INSERT INTO users (username, email, passwordhash, role, createdon, isactive)
--     VALUES (
--         'Admin',
--         'admin@attendance.com',
--         '$2a$11$8vJ5xHQxG9y6FZKZKvKZJeU.KZxKZxKZxKZxKZxKZxKZxKZxKZxKZ.',  -- Admin@123
--         'admin',
--         GETDATE(),
--         1
--     );
-- END
-- GO

-- -- =============================================
-- -- Sample Data (Optional - for testing)
-- -- =============================================

-- -- Insert sample users
-- IF NOT EXISTS (SELECT * FROM users WHERE email = 'john.dev@company.com')
-- BEGIN
--     INSERT INTO users (username, email, passwordhash, role, createdon, isactive)
--     VALUES 
--     ('John Developer', 'john.dev@company.com', '$2a$11$8vJ5xHQxG9y6FZKZKvKZJeU.KZxKZxKZxKZxKZxKZxKZxKZxKZxKZ.', 'developer', GETDATE(), 1),
--     ('Sarah Manager', 'sarah.mgr@company.com', '$2a$11$8vJ5xHQxG9y6FZKZKvKZJeU.KZxKZxKZxKZxKZxKZxKZxKZxKZxKZ.', 'manager', GETDATE(), 1),
--     ('Mike Tester', 'mike.test@company.com', '$2a$11$8vJ5xHQxG9y6FZKZKvKZJeU.KZxKZxKZxKZxKZxKZxKZxKZxKZxKZ.', 'tester', GETDATE(), 1);
-- END
-- GO

-- PRINT 'Database setup completed successfully!';
-- PRINT 'Default Admin User:';
-- PRINT '  Email: admin@attendance.com';
-- PRINT '  Password: Admin@123';
-- GO








-- =============================================
-- Attendance Management System - Enhanced Database Setup
-- =============================================

USE master;
GO

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AttendanceDB')
BEGIN
    CREATE DATABASE AttendanceDB;
END
GO

USE AttendanceDB;
GO

-- =============================================
-- Roles Table (NEW)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'roles')
BEGIN
    CREATE TABLE roles (
        roleid INT IDENTITY(1,1) PRIMARY KEY,
        rolename NVARCHAR(50) NOT NULL UNIQUE,
        description NVARCHAR(200) NULL,
        requiresselfie BIT NOT NULL DEFAULT 0,
        isactive BIT NOT NULL DEFAULT 1,
        createdon DATETIME NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_roles_rolename ON roles(rolename);
END
GO

-- =============================================
-- Holidays Table (NEW)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'holidays')
BEGIN
    CREATE TABLE holidays (
        holidayid INT IDENTITY(1,1) PRIMARY KEY,
        holidayname NVARCHAR(100) NOT NULL,
        holidaydate DATE NOT NULL UNIQUE,
        description NVARCHAR(200) NULL,
        isactive BIT NOT NULL DEFAULT 1,
        createdon DATETIME NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_holidays_date ON holidays(holidaydate);
END
GO

-- =============================================
-- Users Table (UPDATED)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'users')
BEGIN
    CREATE TABLE users (
        userid INT IDENTITY(1,1) PRIMARY KEY,
        username NVARCHAR(100) NOT NULL,
        email NVARCHAR(100) NOT NULL UNIQUE,
        passwordhash NVARCHAR(255) NOT NULL,
        role NVARCHAR(50) NOT NULL,
        roleid INT NULL,  -- NEW: Foreign key to roles table
        deviceid NVARCHAR(255) NULL,
        macaddress NVARCHAR(255) NULL,  -- NEW: MAC address field
        lastseen DATETIME NULL,
        createdon DATETIME NOT NULL DEFAULT GETDATE(),
        isactive BIT NOT NULL DEFAULT 1,
        
        -- Foreign key constraint
        CONSTRAINT FK_users_roles FOREIGN KEY (roleid) 
            REFERENCES roles(roleid) ON DELETE SET NULL
    );

    CREATE INDEX IX_users_email ON users(email);
    CREATE INDEX IX_users_role ON users(role);
    CREATE INDEX IX_users_roleid ON users(roleid);
END
ELSE
BEGIN
    -- Add new columns if table already exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('users') AND name = 'macaddress')
    BEGIN
        ALTER TABLE users ADD macaddress NVARCHAR(255) NULL;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('users') AND name = 'roleid')
    BEGIN
        ALTER TABLE users ADD roleid INT NULL;
        ALTER TABLE users ADD CONSTRAINT FK_users_roles FOREIGN KEY (roleid) 
            REFERENCES roles(roleid) ON DELETE SET NULL;
    END
END
GO

-- =============================================
-- Attendance Table (UPDATED)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'attendance')
BEGIN
    CREATE TABLE attendance (
        attendanceid INT IDENTITY(1,1) PRIMARY KEY,
        userid INT NOT NULL,
        attendancedate DATE NOT NULL,
        
        -- Check-In Details
        intime TIME NULL,
        intimedatetime DATETIME NULL,
        inlatitude DECIMAL(10,7) NULL,
        inlongitude DECIMAL(10,7) NULL,
        inlocationaddress NVARCHAR(500) NULL,
        inselfie NVARCHAR(500) NULL,
        inbiometric NVARCHAR(MAX) NULL,  -- NEW: Biometric data for check-in
        
        -- Check-Out Details
        outtime TIME NULL,
        outtimedatetime DATETIME NULL,
        outlatitude DECIMAL(10,7) NULL,
        outlongitude DECIMAL(10,7) NULL,
        outlocationaddress NVARCHAR(500) NULL,
        outselfie NVARCHAR(500) NULL,
        outbiometric NVARCHAR(MAX) NULL,  -- NEW: Biometric data for check-out
        
        -- Calculated Fields
        totalhours DECIMAL(5,2) NULL,
        
        -- Audit Fields
        createdon DATETIME NOT NULL DEFAULT GETDATE(),
        updatedon DATETIME NULL,
        
        -- Foreign Key
        CONSTRAINT FK_attendance_users FOREIGN KEY (userid) 
            REFERENCES users(userid) ON DELETE CASCADE,
        
        -- Unique Constraint - One attendance per user per day
        CONSTRAINT UQ_attendance_user_date UNIQUE(userid, attendancedate)
    );

    CREATE INDEX IX_attendance_userid ON attendance(userid);
    CREATE INDEX IX_attendance_date ON attendance(attendancedate);
END
ELSE
BEGIN
    -- Add new biometric columns if table already exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('attendance') AND name = 'inbiometric')
    BEGIN
        ALTER TABLE attendance ADD inbiometric NVARCHAR(MAX) NULL;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('attendance') AND name = 'outbiometric')
    BEGIN
        ALTER TABLE attendance ADD outbiometric NVARCHAR(MAX) NULL;
    END
END
GO

-- =============================================
-- Insert Default Roles
-- =============================================
IF NOT EXISTS (SELECT * FROM roles WHERE rolename = 'admin')
BEGIN
    INSERT INTO roles (rolename, description, requiresselfie, isactive, createdon)
    VALUES 
        ('admin', 'System Administrator', 0, 1, GETDATE()),
        ('manager', 'Department Manager', 1, 1, GETDATE()),
        ('supervisor', 'Team Supervisor', 1, 1, GETDATE()),
        ('developer', 'Software Developer', 1, 1, GETDATE()),
        ('tester', 'Quality Assurance Tester', 1, 1, GETDATE()),
        ('hr', 'Human Resources', 0, 1, GETDATE()),
        ('employee', 'General Employee', 1, 1, GETDATE());
END
GO

-- =============================================
-- Insert Default Admin User
-- Password: Admin@123
-- =============================================
IF NOT EXISTS (SELECT * FROM users WHERE email = 'admin@attendance.com')
BEGIN
    -- Get admin role id
    DECLARE @adminRoleId INT;
    SELECT @adminRoleId = roleid FROM roles WHERE rolename = 'admin';
    
    INSERT INTO users (username, email, passwordhash, role, roleid, createdon, isactive)
    VALUES (
        'Admin',
        'admin@attendance.com',
        '$2a$11$8vJ5xHQxG9y6FZKZKvKZJeU.KZxKZxKZxKZxKZxKZxKZxKZxKZxKZ.',  -- Admin@123
        'admin',
        @adminRoleId,
        GETDATE(),
        1
    );
END
GO

-- =============================================
-- Sample Holiday Data (Optional)
-- =============================================
IF NOT EXISTS (SELECT * FROM holidays WHERE holidaydate = '2026-01-26')
BEGIN
    INSERT INTO holidays (holidayname, holidaydate, description, isactive, createdon)
    VALUES 
        ('Republic Day', '2026-01-26', 'National Holiday', 1, GETDATE()),
        ('Holi', '2026-03-06', 'Festival of Colors', 1, GETDATE()),
        ('Independence Day', '2026-08-15', 'National Holiday', 1, GETDATE()),
        ('Diwali', '2026-10-19', 'Festival of Lights', 1, GETDATE()),
        ('Christmas', '2026-12-25', 'Christmas Day', 1, GETDATE());
END
GO

PRINT '=============================================';
PRINT 'Database setup completed successfully!';
PRINT '=============================================';
PRINT 'New Tables Created:';
PRINT '  - roles (with selfie requirement flag)';
PRINT '  - holidays (for holiday management)';
PRINT '';
PRINT 'Updated Tables:';
PRINT '  - users (added macaddress and roleid fields)';
PRINT '  - attendance (added inbiometric and outbiometric fields)';
PRINT '';
PRINT 'Default Admin User:';
PRINT '  Email: admin@attendance.com';
PRINT '  Password: Admin@123';
PRINT '';
PRINT 'Default Roles: admin, manager, supervisor, developer, tester, hr, employee';
PRINT '=============================================';
GO