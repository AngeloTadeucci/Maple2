using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class AddHomePropertiesToLayouts : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<byte>(
                name: "Background",
                table: "home-layout",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<byte>(
                name: "Camera",
                table: "home-layout",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<byte>(
                name: "Lighting",
                table: "home-layout",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte) 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "Background",
                table: "home-layout");

            migrationBuilder.DropColumn(
                name: "Camera",
                table: "home-layout");

            migrationBuilder.DropColumn(
                name: "Lighting",
                table: "home-layout");
        }
    }
}
