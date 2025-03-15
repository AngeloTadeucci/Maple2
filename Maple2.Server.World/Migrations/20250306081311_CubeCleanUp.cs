using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class CubeCleanUp : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "HousingCategory",
                table: "ugcmap-cube");

            migrationBuilder.DropColumn(
                name: "HousingCategory",
                table: "home-layout-cube");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "nurturing",
                newName: "InteractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "InteractId",
                table: "nurturing",
                newName: "ItemId");

            migrationBuilder.AddColumn<int>(
                name: "HousingCategory",
                table: "ugcmap-cube",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HousingCategory",
                table: "home-layout-cube",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
