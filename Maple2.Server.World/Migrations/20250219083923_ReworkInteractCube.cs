using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class ReworkInteractCube : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "CubePortalSettings",
                table: "ugcmap-cube");

            migrationBuilder.DropColumn(
                name: "CubePortalSettings",
                table: "home-layout-cube");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
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
    }
}
