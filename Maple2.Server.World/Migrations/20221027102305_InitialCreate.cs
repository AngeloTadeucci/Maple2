using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    public partial class InitialCreate : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MachineId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    MaxCharacters = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    PrestigeLevel = table.Column<int>(type: "int", nullable: false),
                    PrestigeExp = table.Column<long>(type: "bigint", nullable: false),
                    Trophy = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PremiumTime = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Account", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "item",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<short>(type: "smallint", nullable: false),
                    Group = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TimeChangedOption = table.Column<int>(type: "int", nullable: false),
                    RemainUses = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UnlockTime = table.Column<long>(type: "bigint", nullable: false),
                    GlamorForges = table.Column<short>(type: "smallint", nullable: false),
                    Appearance = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stats = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Enchant = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LimitBreak = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Transfer = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Socket = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoupleInfo = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Binding = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubType = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Item", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ugcmap",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MapId = table.Column<int>(type: "int", nullable: false),
                    Indoor = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    ApartmentNumber = table.Column<int>(type: "int", nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ugcmap", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "character",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gender = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Job = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short) 1),
                    SkinColor = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MapId = table.Column<int>(type: "int", nullable: false),
                    Experience = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Profile = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cooldown = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeleteTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Character", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "home",
                columns: table => new {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Area = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte) 4),
                    Height = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte) 3),
                    CurrentArchitectScore = table.Column<int>(type: "int", nullable: false),
                    ArchitectScore = table.Column<int>(type: "int", nullable: false),
                    Background = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Lighting = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Camera = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Passcode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Permissions = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Home", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Home_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "item-storage",
                columns: table => new {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Meso = table.Column<long>(type: "bigint", nullable: false),
                    Expand = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_item-storage", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_item-storage_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ugcmap-cube",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UgcMapId = table.Column<long>(type: "bigint", nullable: false),
                    X = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Y = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Z = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Rotation = table.Column<float>(type: "float", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Template = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table => {
                    table.PrimaryKey("PK_ugcmap-cube", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ugcmap-cube_ugcmap_UgcMapId",
                        column: x => x.UgcMapId,
                        principalTable: "ugcmap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "buddy",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    BuddyId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Buddy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buddy_Character_BuddyId",
                        column: x => x.BuddyId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Buddy_Character_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "character-unlock",
                columns: table => new {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    Maps = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Taxis = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Titles = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Emotes = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stamps = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Expand = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_character-unlock", x => x.CharacterId);
                    table.ForeignKey(
                        name: "FK_character-unlock_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "club",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    LeaderId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Club", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Club_Character_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "skill-tab",
                columns: table => new {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Skills = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table => {
                    table.PrimaryKey("PK_skill-tab", x => new { x.CharacterId, x.Id });
                    table.UniqueConstraint("AK_skill-tab_Id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_skill-tab_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "club-member",
                columns: table => new {
                    ClubId = table.Column<long>(type: "bigint", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_club-member", x => new { x.ClubId, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_club-member_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_club-member_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "character-config",
                columns: table => new {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    KeyBinds = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HotBars = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SkillMacros = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Wardrobes = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatAllocation = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SkillBook_MaxSkillTabs = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    SkillBook_ActiveSkillTabId = table.Column<long>(type: "bigint", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_character-config", x => x.CharacterId);
                    table.ForeignKey(
                        name: "FK_character-config_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_character-config_skill-tab_SkillBook_ActiveSkillTabId",
                        column: x => x.SkillBook_ActiveSkillTabId,
                        principalTable: "skill-tab",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Account_Username",
                table: "Account",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buddy_BuddyId",
                table: "Buddy",
                column: "BuddyId");

            migrationBuilder.CreateIndex(
                name: "IX_Buddy_OwnerId",
                table: "Buddy",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_AccountId",
                table: "Character",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_Name",
                table: "Character",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_character-config_SkillBook_ActiveSkillTabId",
                table: "character-config",
                column: "SkillBook_ActiveSkillTabId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Club_LeaderId",
                table: "Club",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Club_Name",
                table: "Club",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_club-member_CharacterId",
                table: "club-member",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_skill-tab_CharacterId",
                table: "skill-tab",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ugcmap_MapId",
                table: "ugcmap",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_ugcmap_OwnerId",
                table: "ugcmap",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ugcmap-cube_UgcMapId",
                table: "ugcmap-cube",
                column: "UgcMapId");

            migrationBuilder.Sql("ALTER TABLE `account` AUTO_INCREMENT = 10000000000");
            migrationBuilder.Sql("ALTER TABLE `character` AUTO_INCREMENT = 20000000000");
            migrationBuilder.Sql("ALTER TABLE `club` AUTO_INCREMENT = 30000000000");
            migrationBuilder.Sql("ALTER TABLE `buddy` AUTO_INCREMENT = 40000000000");
            migrationBuilder.Sql("ALTER TABLE `ugcmap` AUTO_INCREMENT = 50000000000");

            // potentially large tables
            migrationBuilder.Sql("ALTER TABLE `ugcmap-cube` AUTO_INCREMENT = 1000000000000");
            migrationBuilder.Sql("ALTER TABLE `item` AUTO_INCREMENT = 2000000000000");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "Buddy");

            migrationBuilder.DropTable(
                name: "character-config");

            migrationBuilder.DropTable(
                name: "character-unlock");

            migrationBuilder.DropTable(
                name: "club-member");

            migrationBuilder.DropTable(
                name: "Home");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "item-storage");

            migrationBuilder.DropTable(
                name: "ugcmap-cube");

            migrationBuilder.DropTable(
                name: "skill-tab");

            migrationBuilder.DropTable(
                name: "Club");

            migrationBuilder.DropTable(
                name: "ugcmap");

            migrationBuilder.DropTable(
                name: "Character");

            migrationBuilder.DropTable(
                name: "Account");
        }
    }
}
