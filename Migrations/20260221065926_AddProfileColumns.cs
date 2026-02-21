using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace attendance_api.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                column: "passwordhash",
                value: "$2a$11$pGkRLCj7ZreITqfe5AgjNOE4pp/kaU2od0MpMrHy4sgvkiA5x3RLq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                column: "passwordhash",
                value: "$2a$11$e9fReega9mXPqzBBcZ98dO8gCFqw4k86GLdY.vWDFpHrIx6Tyhc7a");
        }
    }
}
