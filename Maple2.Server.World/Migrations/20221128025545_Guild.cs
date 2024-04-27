using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    public partial class Guild : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "guild",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Emblem = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notice = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Focus = table.Column<int>(type: "int", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    Funds = table.Column<int>(type: "int", nullable: false),
                    HouseRank = table.Column<int>(type: "int", nullable: false),
                    HouseTheme = table.Column<int>(type: "int", nullable: false),
                    Ranks = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Buffs = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Posters = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Npcs = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LeaderId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_guild", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guild_Character_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "guild-application",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicantId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_guild-application", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guild-application_Character_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guild-application_guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "guild-member",
                columns: table => new {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rank = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    WeeklyContribution = table.Column<int>(type: "int", nullable: false),
                    TotalContribution = table.Column<int>(type: "int", nullable: false),
                    DailyDonationCount = table.Column<int>(type: "int", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CheckinTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DonationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_guild-member", x => new { x.GuildId, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_guild-member_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guild-member_guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_guild_LeaderId",
                table: "guild",
                column: "LeaderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild-application_ApplicantId",
                table: "guild-application",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_guild-application_GuildId",
                table: "guild-application",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_guild-member_CharacterId",
                table: "guild-member",
                column: "CharacterId",
                unique: true);

            migrationBuilder.Sql("ALTER TABLE `guild` AUTO_INCREMENT = 70000000000");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "guild-application");

            migrationBuilder.DropTable(
                name: "guild-member");

            migrationBuilder.DropTable(
                name: "guild");
        }
    }
}
