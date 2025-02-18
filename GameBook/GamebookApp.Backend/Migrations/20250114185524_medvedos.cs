using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamebookApp.Backend.Migrations
{
    /// <inheritdoc />
    public partial class medvedos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BearHP",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BearHP",
                table: "Players");
        }
    }
}
