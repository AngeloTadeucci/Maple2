using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class DeathCount : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "DeathTick",
                table: "character-config");

            migrationBuilder.RenameColumn(
                name: "DeathCount",
                table: "character-config",
                newName: "InstantRevivalCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "InstantRevivalCount",
                table: "character-config",
                newName: "DeathCount");

            migrationBuilder.AddColumn<long>(
                name: "DeathTick",
                table: "character-config",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
