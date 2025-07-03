using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class CubeInteract : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "CubeSettings",
                table: "ugcmap-cube",
                newName: "Interact");

            migrationBuilder.RenameColumn(
                name: "CubeSettings",
                table: "home-layout-cube",
                newName: "Interact");

            migrationBuilder.AddColumn<string>(
                name: "CubePortalSettings",
                table: "ugcmap-cube",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CubePortalSettings",
                table: "home-layout-cube",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "CubePortalSettings",
                table: "ugcmap-cube");

            migrationBuilder.DropColumn(
                name: "CubePortalSettings",
                table: "home-layout-cube");

            migrationBuilder.RenameColumn(
                name: "Interact",
                table: "ugcmap-cube",
                newName: "CubeSettings");

            migrationBuilder.RenameColumn(
                name: "Interact",
                table: "home-layout-cube",
                newName: "CubeSettings");
        }
    }
}
