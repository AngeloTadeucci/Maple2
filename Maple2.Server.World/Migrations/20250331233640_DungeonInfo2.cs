using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class DungeonInfo2 : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "WeeklyResetTime",
                table: "dungeon-record",
                newName: "UnionCooldownTime");

            migrationBuilder.RenameColumn(
                name: "WeeklyRecord",
                table: "dungeon-record",
                newName: "CurrentRecord");

            migrationBuilder.RenameColumn(
                name: "WeeklyClears",
                table: "dungeon-record",
                newName: "Flag");

            migrationBuilder.RenameColumn(
                name: "ExtraWeeklyClears",
                table: "dungeon-record",
                newName: "ExtraCurrentSubClears");

            migrationBuilder.RenameColumn(
                name: "ExtraDailyClears",
                table: "dungeon-record",
                newName: "ExtraCurrentClears");

            migrationBuilder.RenameColumn(
                name: "DailyClears",
                table: "dungeon-record",
                newName: "CurrentSubClears");

            migrationBuilder.AddColumn<DateTime>(
                name: "CooldownTime",
                table: "dungeon-record",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<byte>(
                name: "CurrentClears",
                table: "dungeon-record",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte) 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "CooldownTime",
                table: "dungeon-record");

            migrationBuilder.DropColumn(
                name: "CurrentClears",
                table: "dungeon-record");

            migrationBuilder.RenameColumn(
                name: "UnionCooldownTime",
                table: "dungeon-record",
                newName: "WeeklyResetTime");

            migrationBuilder.RenameColumn(
                name: "Flag",
                table: "dungeon-record",
                newName: "WeeklyClears");

            migrationBuilder.RenameColumn(
                name: "ExtraCurrentSubClears",
                table: "dungeon-record",
                newName: "ExtraWeeklyClears");

            migrationBuilder.RenameColumn(
                name: "ExtraCurrentClears",
                table: "dungeon-record",
                newName: "ExtraDailyClears");

            migrationBuilder.RenameColumn(
                name: "CurrentSubClears",
                table: "dungeon-record",
                newName: "DailyClears");

            migrationBuilder.RenameColumn(
                name: "CurrentRecord",
                table: "dungeon-record",
                newName: "WeeklyRecord");
        }
    }
}
