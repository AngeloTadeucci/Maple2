using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class AddInteractIdToCubes : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<int>(
                name: "HousingCategory",
                table: "ugcmap-cube",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InteractId",
                table: "ugcmap-cube",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "HousingCategory",
                table: "home-layout-cube",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InteractId",
                table: "home-layout-cube",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "HousingCategory",
                table: "ugcmap-cube");

            migrationBuilder.DropColumn(
                name: "InteractId",
                table: "ugcmap-cube");

            migrationBuilder.DropColumn(
                name: "HousingCategory",
                table: "home-layout-cube");

            migrationBuilder.DropColumn(
                name: "InteractId",
                table: "home-layout-cube");
        }
    }
}
