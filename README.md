# Attendance Management System API - v2.0

A comprehensive ASP.NET Core Web API for employee attendance tracking with geolocation and selfie verification.

## 🚀 Recent Updates (v2.0)

**Major Changes:**
- ✅ Auto-populated UserId from JWT token (all endpoints)
- ✅ Auto-captured In-Time and Out-Time using server time
- ✅ Required date range validation (max 31 days) for summary APIs
- ❌ Removed `exportusersummary` endpoint
- ❌ Removed `todaystatus` endpoint
- ✅ Token removed from register API response

**See [API_MODIFICATIONS.md](API_MODIFICATIONS.md) for complete details**

---

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Database Schema](#database-schema)

---

## ✨ Features

- 🔐 **JWT Authentication** with role-based access control
- 📍 **Geolocation Tracking** for check-in/check-out
- 📸 **Selfie Verification** at attendance marking
- ⏰ **Automatic Time Capture** for in-time and out-time
- 📊 **Attendance Reports** with date range filtering
- 👥 **Multi-Role Support** (admin, manager, employee, etc.)
- 🔒 **Device Binding** for security
- 📱 **Mobile-Friendly** API design

---

## 🛠️ Tech Stack

- ASP.NET Core 8.0
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt for password hashing
- QuestPDF for PDF generation

---

## 📦 Prerequisites

- .NET 8.0 SDK or later
- SQL Server (2019 or later)
- Visual Studio 2022 / VS Code / Rider

---

## 🔧 Installation

### 1. Clone the Repository
```bash
git clone <repository-url>
cd attendance_api
```

### 2. Configure Database
Update `appsettings.json` with your SQL Server connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AttendanceDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 3. Run Database Setup
Execute the SQL script in `Database/setup_database.sql` to create the database and tables.

### 4. Build and Run
```bash
dotnet restore
dotnet build
dotnet run
```

The API will be available at: `https://localhost:7001` or `http://localhost:5000`

---

## 🔌 API Endpoints

### Authentication APIs

#### 1. Register
```http
POST /api/auth/register
Content-Type: application/json
```

**Request Body:**
```json
{
    "userName": "John Doe",
    "email": "john@example.com",
    "password": "SecurePass123!",
    "role": "employee"
}
```

**Response:**
```json
{
    "success": true,
    "message": "Registration successful",
    "data": {
        "userId": 1,
        "userName": "John Doe",
        "email": "john@example.com",
        "role": "employee",
        "token": null,
        "message": "Welcome! Please login to continue."
    }
}
```

**Note:** Token is null on registration. User must login separately.

---

#### 2. Login
```http
POST /api/auth/login
Content-Type: application/json
```

**Request Body:**
```json
{
    "email": "john@example.com",
    "password": "SecurePass123!",
    "deviceId": "unique-device-id"
}
```

**Response:**
```json
{
    "success": true,
    "message": "Login successful",
    "data": {
        "userId": 1,
        "userName": "John Doe",
        "email": "john@example.com",
        "role": "employee",
        "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        "message": "Welcome back!"
    }
}
```

---

### Attendance APIs

#### 3. Mark In (Check-In)
```http
POST /api/attendance/markin
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Request (Form-Data):**
- `attendanceDate`: `2026-02-09` (Required, date only)
- `latitude`: `28.7041` (Required)
- `longitude`: `77.1025` (Required)
- `locationAddress`: `"Office Location"` (Required)
- `selfieImage`: `<file>` (Required)

**Response:**
```json
{
    "success": true,
    "message": "Check-in successful",
    "data": {
        "attendanceDate": "09-Feb-2026",
        "inTime": {
            "ticks": "09:30:45"
        },
        "location": "Office Location"
    }
}
```

**Notes:**
- UserId is auto-populated from JWT token
- InTime is auto-captured using current server time

---

#### 4. Mark Out (Check-Out)
```http
POST /api/attendance/markout
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Request (Form-Data):**
- `attendanceDate`: `2026-02-09` (Required, date only)
- `latitude`: `28.7041` (Required)
- `longitude`: `77.1025` (Required)
- `locationAddress`: `"Office Location"` (Required)
- `selfieImage`: `<file>` (Required)

**Response:**
```json
{
    "success": true,
    "message": "Check-out successful",
    "data": {
        "attendanceDate": "09-Feb-2026",
        "inTime": {
            "ticks": "09:30:45"
        },
        "outTime": {
            "ticks": "18:15:30"
        },
        "totalHours": "8.75",
        "location": "Office Location"
    }
}
```

**Notes:**
- UserId is auto-populated from JWT token
- OutTime is auto-captured using current server time
- Must mark-in before marking-out

---

#### 5. User Summary
```http
GET /api/attendance/usersummary?fromDate=2026-01-01&toDate=2026-01-31
Authorization: Bearer <token>
```

**Query Parameters:**
- `fromDate`: `2026-01-01` (Required, YYYY-MM-DD)
- `toDate`: `2026-01-31` (Required, YYYY-MM-DD)

**Validation:**
- Date range cannot exceed 31 days

**Response:**
```json
{
    "success": true,
    "message": "Summary retrieved successfully",
    "data": [
        {
            "attendanceId": 1,
            "userName": "John Doe",
            "role": "employee",
            "attendanceDate": "2026-01-15T00:00:00",
            "inTime": "09:30:45",
            "outTime": "18:15:30",
            "inLocation": "Office Location",
            "outLocation": "Office Location",
            "totalHours": 8.75,
            "status": "Complete"
        }
    ]
}
```

**Notes:**
- UserId is auto-populated from JWT token
- Returns user's own attendance records only

---

#### 6. Admin Summary
```http
GET /api/attendance/adminsummary?role=employee&fromDate=2026-01-01&toDate=2026-01-31
Authorization: Bearer <token>
```

**Query Parameters:**
- `role`: `"employee"` (Required)
- `fromDate`: `2026-01-01` (Required, YYYY-MM-DD)
- `toDate`: `2026-01-31` (Required, YYYY-MM-DD)

**Validation:**
- Date range cannot exceed 31 days
- Requires "admin" role

**Response:**
```json
{
    "success": true,
    "message": "Admin summary retrieved successfully",
    "data": [
        {
            "attendanceId": 1,
            "userName": "John Doe",
            "role": "employee",
            "attendanceDate": "2026-01-15T00:00:00",
            "inTime": "09:30:45",
            "outTime": "18:15:30",
            "inLocation": "Office Location",
            "outLocation": "Office Location",
            "totalHours": 8.75,
            "status": "Complete"
        }
    ]
}
```

**Notes:**
- UserId is auto-populated from JWT token (for tracking)
- Returns all users matching the specified role
- Admin access required

---

#### 7. Export Admin Summary (PDF)
```http
POST /api/attendance/exportadminsummary
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
    "role": "employee",
    "fromDate": "2026-01-01",
    "toDate": "2026-01-31"
}
```

**Response:**
- Binary PDF file download
- Filename: `admin_attendance_summary_20260101_20260131.pdf`

**Notes:**
- Requires "admin" role
- Date range validation applies (max 31 days)

---

## 🔐 Authentication

All attendance endpoints require JWT authentication.

### Header Format:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Acquisition Flow:
1. Register → `/api/auth/register` (no token returned)
2. Login → `/api/auth/login` (token returned)
3. Use token for all subsequent requests

### Token Contents:
- UserId
- Email
- Role
- Expiration time

---

## 🗄️ Database Schema

### Users Table
```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    DeviceId NVARCHAR(255),
    LastSeen DATETIME,
    CreatedOn DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
```

### Attendances Table
```sql
CREATE TABLE Attendances (
    AttendanceId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    AttendanceDate DATE NOT NULL,
    InTime TIME,
    InTimeDateTime DATETIME,
    InLatitude DECIMAL(10,8),
    InLongitude DECIMAL(11,8),
    InLocationAddress NVARCHAR(500),
    InSelfie NVARCHAR(255),
    OutTime TIME,
    OutTimeDateTime DATETIME,
    OutLatitude DECIMAL(10,8),
    OutLongitude DECIMAL(11,8),
    OutLocationAddress NVARCHAR(500),
    OutSelfie NVARCHAR(255),
    TotalHours DECIMAL(5,2),
    CreatedOn DATETIME DEFAULT GETDATE(),
    UpdatedOn DATETIME
);
```

---

## 📄 Supported Roles

- `admin` - Full system access
- `manager` - Team management access
- `supervisor` - Limited oversight access
- `developer` - Development team member
- `tester` - Testing team member
- `hr` - HR department access
- `employee` - Basic employee access

---

## ⚠️ Important Notes

1. **UserId Auto-Population**: All endpoints automatically extract userId from JWT token
2. **Time Auto-Capture**: In-time and out-time are captured using server's current time
3. **Date Format**: Always use `YYYY-MM-DD` format
4. **Date Range Limit**: Summary APIs enforce a maximum 31-day range
5. **Selfie Storage**: Images stored in `wwwroot/uploads/selfies/`
6. **Device Binding**: Each user can login from only one device at a time

---

## 📚 Additional Documentation

- [API Modifications Guide](API_MODIFICATIONS.md) - Complete changelog
- [Setup Guide](SETUP_GUIDE.md) - Detailed installation steps
- [Postman Collection](Postman_Collection.json) - Import for testing

---

## 🐛 Common Errors

### Error: "Invalid token"
- **Cause**: Missing or expired JWT token
- **Solution**: Login again to get a new token

### Error: "Date range cannot exceed one month (31 days)"
- **Cause**: FromDate and ToDate difference > 31 days
- **Solution**: Reduce date range to 31 days or less

### Error: "Please mark check-in first"
- **Cause**: Attempting to mark-out without marking-in
- **Solution**: Call mark-in API first

### Error: "Attendance already marked for today"
- **Cause**: Duplicate check-in/check-out for the same date
- **Solution**: Check existing attendance status first

---

## 📞 Support

For issues and queries, please contact the development team.

---

## 📝 License

This project is proprietary and confidential.

---
