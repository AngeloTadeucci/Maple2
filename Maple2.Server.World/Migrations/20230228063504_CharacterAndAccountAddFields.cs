using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class CharacterAndAccountAddFields : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<short>(
                name: "Channel",
                table: "Character",
                type: "smallint",
                nullable: false,
                defaultValue: (short) 0);

            migrationBuilder.AddColumn<int>(
                name: "ReturnMapId",
                table: "Character",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Online",
                table: "Account",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Character");

            migrationBuilder.DropColumn(
                name: "ReturnMapId",
                table: "Character");

            migrationBuilder.DropColumn(
                name: "Online",
                table: "Account");
        }
    }
}
