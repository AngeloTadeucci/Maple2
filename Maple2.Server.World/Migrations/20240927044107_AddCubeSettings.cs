using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class AddCubeSettings : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "CubeSettings",
                table: "ugcmap-cube",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "HousingCategory",
                table: "ugcmap-cube",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CubeSettings",
                table: "home-layout-cube",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "HousingCategory",
                table: "home-layout-cube",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "CubeSettings",
                table: "ugcmap-cube");

            migrationBuilder.DropColumn(
                name: "HousingCategory",
                table: "ugcmap-cube");

            migrationBuilder.DropColumn(
                name: "CubeSettings",
                table: "home-layout-cube");

            migrationBuilder.DropColumn(
                name: "HousingCategory",
                table: "home-layout-cube");
        }
    }
}
