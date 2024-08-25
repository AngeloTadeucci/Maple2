using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class StatPoints : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<int>(
                name: "ExplorationProgress",
                table: "character-config",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatPoints",
                table: "character-config",
                type: "json",
                nullable: true,
                defaultValue: "{}")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "ExplorationProgress",
                table: "character-config");

            migrationBuilder.DropColumn(
                name: "StatPoints",
                table: "character-config");
        }
    }
}
