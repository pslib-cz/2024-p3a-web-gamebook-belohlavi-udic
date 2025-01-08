    using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gamebook.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "Files",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "55555555-4444-3333-2222-111111111111", null, "Admin", "ADMIN" },
                    { "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", null, "Author", "AUTHOR" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RoleId", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "99999999-8888-7777-6666-555555555555", 0, "3409c5f4-65f5-41a4-a96a-e8030d7b2037", "admin@example.com", true, false, null, "ADMIN@EXAMPLE.COM", "ADMIN", "AQAAAAIAAYagAAAAEKgn5j1937woZspCeyK9H3JQ49hR+1xiS8gZILsRPMe/fkQUcbYbdpdQ8cUiYV3tdA==", null, false, null, "", false, "admin" });

            migrationBuilder.InsertData(
                table: "Challenges",
                columns: new[] { "ID", "Description", "FailureOutcome", "SuccessOutcome", "Type" },
                values: new object[,]
                {
                    { 1, "Na zemi leží starý svitek s hádankou: 'Co má oči, ale nevidí?'", "Špatná odpověď. Zklamal jsi ducha jeskyně.", "Správně jsi odpověděl! Otevřela se tajná schránka a v ní jsi našel 10 zlaťáků.", "Hádanka" },
                    { 2, "Cestu ti zatarasil divoký vlk. Musíš ho porazit v souboji!", "Vlk tě přemohl a ukousl ti kus zdraví!", "Vlk je poražen! Můžeš pokračovat v cestě.", "Souboj" }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "ID", "Description", "Exits", "Name" },
                values: new object[,]
                {
                    { 1, "You stand at the mouth of a dark and mysterious cave. A chilling draft emanates from within, carrying the scent of damp earth and an unknown, ancient presence. The entrance is shrouded in shadows, making it difficult to discern what lies beyond. You can see a path leading north into the darkness.", "north", "Mysterious Cave Entrance" },
                    { 2, "You enter a dimly lit cavern. The air is heavy and still. Water drips from unseen stalactites, echoing in the silence. You can go back south", "south", "Dark Cavern" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "55555555-4444-3333-2222-111111111111", "99999999-8888-7777-6666-555555555555" });

            migrationBuilder.InsertData(
                table: "Connections",
                columns: new[] { "ID", "ConnectionType", "RoomID1", "RoomID2" },
                values: new object[,]
                {
                    { 1, "north", 1, 2 },
                    { 2, "south", 2, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleId",
                table: "AspNetUsers",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "55555555-4444-3333-2222-111111111111", "99999999-8888-7777-6666-555555555555" });

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Connections",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Connections",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "55555555-4444-3333-2222-111111111111");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "99999999-8888-7777-6666-555555555555");

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Files",
                newName: "FileId");

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    RolesId = table.Column<string>(type: "TEXT", nullable: false),
                    UsersId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleUser_AspNetRoles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersId",
                table: "RoleUser",
                column: "UsersId");
        }
    }
}
