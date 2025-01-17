using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GameBook.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "ID", "Description", "Exits", "Name" },
                values: new object[,]
                {
                    { 1, "Nacházíte se v opuštěném táboře uprostřed džungle. Všude kolem je hustá vegetace.", "východ, západ", "Startovní tábor" },
                    { 2, "Úzká pěšina vede skrz hustou džungli. Slyšíte zvuky divokých zvířat.", "sever, jih", "Lesní cesta" },
                    { 3, "Před vámi se tyčí starý provazový most. Vypadá nebezpečně.", "východ, západ", "Starý most" }
                });

            migrationBuilder.InsertData(
                table: "Connections",
                columns: new[] { "ID", "ConnectionType", "RoomID1", "RoomID2" },
                values: new object[,]
                {
                    { 1, "Jít na východ", 1, 2 },
                    { 2, "Jít na sever", 2, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Connections",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Connections",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "ID",
                keyValue: 3);
        }
    }
}
