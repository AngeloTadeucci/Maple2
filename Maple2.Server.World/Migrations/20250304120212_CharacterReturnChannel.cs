using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class CharacterReturnChannel : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<short>(
                name: "ReturnChannel",
                table: "character",
                type: "smallint",
                nullable: false,
                defaultValue: (short) 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "ReturnChannel",
                table: "character");
        }
    }
}
