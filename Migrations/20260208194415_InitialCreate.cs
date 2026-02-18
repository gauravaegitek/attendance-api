using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace attendance_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    passwordhash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    deviceid = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    lastseen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    createdon = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    isactive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.userid);
                });

            migrationBuilder.CreateTable(
                name: "attendance",
                columns: table => new
                {
                    attendanceid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userid = table.Column<int>(type: "int", nullable: false),
                    attendancedate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    intime = table.Column<TimeSpan>(type: "time", nullable: true),
                    intimedatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    inlatitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    inlongitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    inlocationaddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    inselfie = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    outtime = table.Column<TimeSpan>(type: "time", nullable: true),
                    outtimedatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    outlatitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    outlongitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    outlocationaddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    outselfie = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    totalhours = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    createdon = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updatedon = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance", x => x.attendanceid);
                    table.ForeignKey(
                        name: "FK_attendance_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "userid", "createdon", "deviceid", "email", "isactive", "lastseen", "passwordhash", "role", "username" },
                values: new object[] { 1, new DateTime(2026, 2, 9, 1, 14, 13, 917, DateTimeKind.Local).AddTicks(5331), null, "admin@attendance.com", true, null, "$2a$11$of1J2CuwNhVUzg4X/XXMjej9L2yKqFqVBc1.whkvNX74S777K.aHa", "admin", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_userid_attendancedate",
                table: "attendance",
                columns: new[] { "userid", "attendancedate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
