using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class RemoveSkillCooldown : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "SkillCooldowns",
                table: "character-config");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "SkillCooldowns",
                table: "character-config",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
