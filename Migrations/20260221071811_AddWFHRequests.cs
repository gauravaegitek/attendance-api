using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace attendance_api.Migrations
{
    /// <inheritdoc />
    public partial class AddWFHRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                column: "passwordhash",
                value: "$2a$11$YQxcO.SNcYkYspNawN3bUu9GCv2wCx3XPONoZZ/r6OfQDD7p/YYR2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userid",
                keyValue: 1,
                column: "passwordhash",
                value: "$2a$11$pGkRLCj7ZreITqfe5AgjNOE4pp/kaU2od0MpMrHy4sgvkiA5x3RLq");
        }
    }
}
