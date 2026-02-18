using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace attendance_api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModels_Fresh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "confirmpassword",
                table: "users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "macaddress",
                table: "users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "roleid",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "inbiometric",
                table: "attendance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "outbiometric",
                table: "attendance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "holidays",
                columns: table => new
                {
                    holidayid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    holidayname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    holidaydate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    isactive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    createdon = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_holidays", x => x.holidayid);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    roleid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rolename = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    requiresselfie = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    isactive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    createdon = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.roleid);
                });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                columns: new[] { "confirmpassword", "createdon", "macaddress", "passwordhash", "roleid" },
                values: new object[] { null, new DateTime(2026, 2, 14, 9, 45, 18, 480, DateTimeKind.Local).AddTicks(6160), null, "$2a$11$ncaFWkdH5/SEu2JDk.vonuDkZlGH.7G6UzTWIVDxQFjivzTBGiE9q", null });

            migrationBuilder.CreateIndex(
                name: "IX_users_roleid",
                table: "users",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "IX_holidays_holidaydate",
                table: "holidays",
                column: "holidaydate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_rolename",
                table: "roles",
                column: "rolename",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_users_roles_roleid",
                table: "users",
                column: "roleid",
                principalTable: "roles",
                principalColumn: "roleid",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_roles_roleid",
                table: "users");

            migrationBuilder.DropTable(
                name: "holidays");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropIndex(
                name: "IX_users_roleid",
                table: "users");

            migrationBuilder.DropColumn(
                name: "confirmpassword",
                table: "users");

            migrationBuilder.DropColumn(
                name: "macaddress",
                table: "users");

            migrationBuilder.DropColumn(
                name: "roleid",
                table: "users");

            migrationBuilder.DropColumn(
                name: "inbiometric",
                table: "attendance");

            migrationBuilder.DropColumn(
                name: "outbiometric",
                table: "attendance");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                columns: new[] { "createdon", "passwordhash" },
                values: new object[] { new DateTime(2026, 2, 9, 1, 14, 13, 917, DateTimeKind.Local).AddTicks(5331), "$2a$11$of1J2CuwNhVUzg4X/XXMjej9L2yKqFqVBc1.whkvNX74S777K.aHa" });
        }
    }
}
