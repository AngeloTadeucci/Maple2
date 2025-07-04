using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class GuideRecord : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "DeathPenalty",
                table: "character-config");

            migrationBuilder.AddColumn<int>(
                name: "DeathCount",
                table: "character-config",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "DeathTick",
                table: "character-config",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "GuideRecords",
                table: "character-config",
                type: "json",
                nullable: true,
                defaultValue: "{}")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "DeathCount",
                table: "character-config");

            migrationBuilder.DropColumn(
                name: "DeathTick",
                table: "character-config");

            migrationBuilder.DropColumn(
                name: "GuideRecords",
                table: "character-config");

            migrationBuilder.AddColumn<string>(
                name: "DeathPenalty",
                table: "character-config",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
