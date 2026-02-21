using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace attendance_api.Migrations
{
    /// <inheritdoc />
    public partial class AddWFHAndPerformanceFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "dateofbirth",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "department",
                table: "users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "designation",
                table: "users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "emergencycontact",
                table: "users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profilephoto",
                table: "users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PerformanceReviews",
                columns: table => new
                {
                    reviewid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userid = table.Column<int>(type: "int", nullable: false),
                    reviewmonth = table.Column<int>(type: "int", nullable: false),
                    reviewyear = table.Column<int>(type: "int", nullable: false),
                    attendancescore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    manualscore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    finalscore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    grade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "C"),
                    reviewercomments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    reviewedbyuserid = table.Column<int>(type: "int", nullable: true),
                    createdon = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReviews", x => x.reviewid);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFHRequests",
                columns: table => new
                {
                    wfhid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userid = table.Column<int>(type: "int", nullable: false),
                    wfhdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    approvedbyuserid = table.Column<int>(type: "int", nullable: true),
                    approvedon = table.Column<DateTime>(type: "datetime2", nullable: true),
                    rejectionreason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    createdon = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFHRequests", x => x.wfhid);
                    table.ForeignKey(
                        name: "FK_WFHRequests_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                columns: new[] { "address", "dateofbirth", "department", "designation", "emergencycontact", "passwordhash", "phone", "profilephoto" },
                values: new object[] { null, null, null, null, null, "$2a$11$e9fReega9mXPqzBBcZ98dO8gCFqw4k86GLdY.vWDFpHrIx6Tyhc7a", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_userid_reviewmonth_reviewyear",
                table: "PerformanceReviews",
                columns: new[] { "userid", "reviewmonth", "reviewyear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WFHRequests_userid_wfhdate",
                table: "WFHRequests",
                columns: new[] { "userid", "wfhdate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerformanceReviews");

            migrationBuilder.DropTable(
                name: "WFHRequests");

            migrationBuilder.DropColumn(
                name: "address",
                table: "users");

            migrationBuilder.DropColumn(
                name: "dateofbirth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "department",
                table: "users");

            migrationBuilder.DropColumn(
                name: "designation",
                table: "users");

            migrationBuilder.DropColumn(
                name: "emergencycontact",
                table: "users");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "users");

            migrationBuilder.DropColumn(
                name: "profilephoto",
                table: "users");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                column: "passwordhash",
                value: "$2a$11$lHg4uIoUoQB47u6MCIPqAOntnHdDPvWy0sHPpJvrAayIo.Tvb4qZq");
        }
    }
}
