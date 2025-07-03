using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class DungeonInfo : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "DungeonRankRewards",
                table: "character-unlock",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dungeon-record",
                columns: table => new {
                    DungeonId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    ClearTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalClears = table.Column<int>(type: "int", nullable: false),
                    DailyClears = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    WeeklyClears = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    LifetimeRecord = table.Column<short>(type: "smallint", nullable: false),
                    WeeklyRecord = table.Column<short>(type: "smallint", nullable: false),
                    ExtraDailyClears = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ExtraWeeklyClears = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DailyResetTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    WeeklyResetTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_dungeon-record", x => new { x.OwnerId, x.DungeonId });
                    table.ForeignKey(
                        name: "FK_dungeon-record_character_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "dungeon-record");

            migrationBuilder.DropColumn(
                name: "DungeonRankRewards",
                table: "character-unlock");
        }
    }
}
