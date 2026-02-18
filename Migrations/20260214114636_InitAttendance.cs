using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace attendance_api.Migrations
{
    /// <inheritdoc />
    public partial class InitAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "roleid", "createdon", "description", "isactive", "rolename" },
                values: new object[] { 1, new DateTime(2026, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "System Admin", true, "admin" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                columns: new[] { "createdon", "passwordhash", "roleid" },
                values: new object[] { new DateTime(2026, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "$2a$11$lHg4uIoUoQB47u6MCIPqAOntnHdDPvWy0sHPpJvrAayIo.Tvb4qZq", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "roleid",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                columns: new[] { "createdon", "passwordhash", "roleid" },
                values: new object[] { new DateTime(2026, 2, 14, 9, 45, 18, 480, DateTimeKind.Local).AddTicks(6160), "$2a$11$ncaFWkdH5/SEu2JDk.vonuDkZlGH.7G6UzTWIVDxQFjivzTBGiE9q", null });
        }
    }
}
