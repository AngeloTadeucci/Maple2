using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations
{
    /// <inheritdoc />
    public partial class SurvivalStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActiveGoldPass",
                table: "Account",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "SurvivalExp",
                table: "Account",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SurvivalGoldLevelRewardClaimed",
                table: "Account",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurvivalLevel",
                table: "Account",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurvivalSilverLevelRewardClaimed",
                table: "Account",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveGoldPass",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "SurvivalExp",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "SurvivalGoldLevelRewardClaimed",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "SurvivalLevel",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "SurvivalSilverLevelRewardClaimed",
                table: "Account");
        }
    }
}
