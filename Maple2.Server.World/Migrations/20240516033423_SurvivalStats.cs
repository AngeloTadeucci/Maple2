using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class SurvivalStats : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<bool>(
                name: "ActiveGoldPass",
                table: "account",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "SurvivalExp",
                table: "account",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SurvivalGoldLevelRewardClaimed",
                table: "account",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurvivalLevel",
                table: "account",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurvivalSilverLevelRewardClaimed",
                table: "account",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "ActiveGoldPass",
                table: "account");

            migrationBuilder.DropColumn(
                name: "SurvivalExp",
                table: "account");

            migrationBuilder.DropColumn(
                name: "SurvivalGoldLevelRewardClaimed",
                table: "account");

            migrationBuilder.DropColumn(
                name: "SurvivalLevel",
                table: "account");

            migrationBuilder.DropColumn(
                name: "SurvivalSilverLevelRewardClaimed",
                table: "account");
        }
    }
}
