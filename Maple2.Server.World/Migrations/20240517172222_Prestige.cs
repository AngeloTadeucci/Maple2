using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations
{
    /// <inheritdoc />
    public partial class Prestige : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PrestigeCurrentExp",
                table: "Account",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "PrestigeLevelsGained",
                table: "Account",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrestigeMissions",
                table: "Account",
                type: "json",
                defaultValue: "[]",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PrestigeRewardsClaimed",
                table: "Account",
                type: "json",
                defaultValue: "[]",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrestigeCurrentExp",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "PrestigeLevelsGained",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "PrestigeMissions",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "PrestigeRewardsClaimed",
                table: "Account");
        }
    }
}
