-- ============================================================
-- Attendance API - New Features Migration Script
-- Run this on SQL Server (SSMS) on AttendanceDB
-- Date: February 2026
-- ============================================================

USE AttendanceDB;
GO

-- ─────────────────────────────────────────────────────────────
-- STEP 1: Add Profile columns to Users table
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'phone')
    ALTER TABLE users ADD phone NVARCHAR(20) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'department')
    ALTER TABLE users ADD department NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'designation')
    ALTER TABLE users ADD designation NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'profilephoto')
    ALTER TABLE users ADD profilephoto NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'dateofbirth')
    ALTER TABLE users ADD dateofbirth DATE NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'address')
    ALTER TABLE users ADD address NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'emergencycontact')
    ALTER TABLE users ADD emergencycontact NVARCHAR(100) NULL;

PRINT '✅ Step 1: Users table profile columns added';
GO

-- ─────────────────────────────────────────────────────────────
-- STEP 2: Create WFHRequests table
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'WFHRequests')
BEGIN
    CREATE TABLE WFHRequests (
        wfhid               INT             PRIMARY KEY IDENTITY(1,1),
        userid              INT             NOT NULL,
        wfhdate             DATE            NOT NULL,
        reason              NVARCHAR(500)   NOT NULL,
        status              NVARCHAR(20)    NOT NULL DEFAULT 'Pending',
        approvedbyuserid    INT             NULL,
        approvedon          DATETIME        NULL,
        rejectionreason     NVARCHAR(500)   NULL,
        createdon           DATETIME        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_WFHRequests_Users
            FOREIGN KEY (userid) REFERENCES users(userid) ON DELETE CASCADE,

        CONSTRAINT UQ_WFHRequests_UserDate
            UNIQUE (userid, wfhdate)
    );

    CREATE INDEX IX_WFHRequests_UserId  ON WFHRequests(userid);
    CREATE INDEX IX_WFHRequests_Date    ON WFHRequests(wfhdate);
    CREATE INDEX IX_WFHRequests_Status  ON WFHRequests(status);

    PRINT '✅ Step 2: WFHRequests table created';
END
ELSE
    PRINT '⚠️  Step 2: WFHRequests table already exists - skipped';
GO

-- ─────────────────────────────────────────────────────────────
-- STEP 3: Create PerformanceReviews table
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PerformanceReviews')
BEGIN
    CREATE TABLE PerformanceReviews (
        reviewid            INT             PRIMARY KEY IDENTITY(1,1),
        userid              INT             NOT NULL,
        reviewmonth         INT             NOT NULL,
        reviewyear          INT             NOT NULL,
        attendancescore     DECIMAL(5,2)    NOT NULL DEFAULT 0,
        manualscore         DECIMAL(5,2)    NULL,
        finalscore          DECIMAL(5,2)    NOT NULL DEFAULT 0,
        grade               NVARCHAR(5)     NOT NULL DEFAULT 'C',
        reviewercomments    NVARCHAR(1000)  NULL,
        reviewedbyuserid    INT             NULL,
        createdon           DATETIME        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_PerformanceReviews_Users
            FOREIGN KEY (userid) REFERENCES users(userid) ON DELETE CASCADE,

        CONSTRAINT UQ_PerformanceReviews_UserMonthYear
            UNIQUE (userid, reviewmonth, reviewyear)
    );

    CREATE INDEX IX_PerfReviews_UserId    ON PerformanceReviews(userid);
    CREATE INDEX IX_PerfReviews_MonthYear ON PerformanceReviews(reviewmonth, reviewyear);

    PRINT '✅ Step 3: PerformanceReviews table created';
END
ELSE
    PRINT '⚠️  Step 3: PerformanceReviews table already exists - skipped';
GO

-- ─────────────────────────────────────────────────────────────
-- STEP 4: Verify — Show all tables
-- ─────────────────────────────────────────────────────────────
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

PRINT '✅ Migration completed successfully!';
GO