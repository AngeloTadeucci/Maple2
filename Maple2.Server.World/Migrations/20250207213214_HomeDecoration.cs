using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class HomeDecoration : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<long>(
                name: "DecorationExp",
                table: "home",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DecorationLevel",
                table: "home",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DecorationRewardTimestamp",
                table: "home",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "InteriorRewardsClaimed",
                table: "home",
                type: "json",
                defaultValue: "[]",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "DecorationExp",
                table: "home");

            migrationBuilder.DropColumn(
                name: "DecorationLevel",
                table: "home");

            migrationBuilder.DropColumn(
                name: "DecorationRewardTimestamp",
                table: "home");

            migrationBuilder.DropColumn(
                name: "InteriorRewardsClaimed",
                table: "home");
        }
    }
}
