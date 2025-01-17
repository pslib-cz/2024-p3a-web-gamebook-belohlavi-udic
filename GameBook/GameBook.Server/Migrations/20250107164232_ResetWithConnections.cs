using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameBook.Server.Migrations
{
    /// <inheritdoc />
    public partial class ResetWithConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Connections",
                keyColumn: "ID",
                keyValue: 2,
                columns: new[] { "RoomID1", "RoomID2" },
                values: new object[] { 3, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Connections",
                keyColumn: "ID",
                keyValue: 2,
                columns: new[] { "RoomID1", "RoomID2" },
                values: new object[] { 2, 3 });
        }
    }
}
