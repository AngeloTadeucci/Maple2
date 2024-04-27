using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    public partial class LapenshardAndQuest : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "Quests",
                table: "character-unlock",
                type: "json",
                nullable: false,
                defaultValue: "[]")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Lapenshards",
                table: "character-config",
                type: "json",
                nullable: true,
                defaultValue: "{}")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "Quests",
                table: "character-unlock");

            migrationBuilder.DropColumn(
                name: "Lapenshards",
                table: "character-config");
        }
    }
}
