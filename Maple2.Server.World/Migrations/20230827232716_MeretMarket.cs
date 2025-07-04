using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class MeretMarket : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "premium-market-item",
                columns: table => new {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    TabId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    BonusQuantity = table.Column<int>(type: "int", nullable: false),
                    ItemDuration = table.Column<int>(type: "int", nullable: false),
                    CurrencyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    SalePrice = table.Column<long>(type: "bigint", nullable: false),
                    SellBeginTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SellEndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SalesCount = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    JobRequirement = table.Column<int>(type: "int", nullable: false),
                    RequireAchievementId = table.Column<int>(type: "int", nullable: false),
                    RequireAchievementRank = table.Column<int>(type: "int", nullable: false),
                    BannerLabel = table.Column<int>(type: "int", nullable: false),
                    BannerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PromoData = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RestockUnavailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequireMinLevel = table.Column<short>(type: "smallint", nullable: false),
                    RequireMaxLevel = table.Column<short>(type: "smallint", nullable: false),
                    PcCafe = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShowSaleTime = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_premium-market-item", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "system-banner",
                columns: table => new {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Function = table.Column<int>(type: "int", nullable: false),
                    FunctionParameter = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Url = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<int>(type: "int", nullable: false),
                    BeginTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_system-banner", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ugc-market-item",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    SalesCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ListingEndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PromotionEndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    CharacterName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tags = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Look = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ugc-market-item", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("ALTER TABLE `ugc-market-item` AUTO_INCREMENT = 90000000001");
            migrationBuilder.Sql("ALTER TABLE `premium-market-item` AUTO_INCREMENT = 100000000000");
            // System Banners are intentionally lower increment
            migrationBuilder.Sql("ALTER TABLE `system-banner` AUTO_INCREMENT = 10000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "premium-market-item");

            migrationBuilder.DropTable(
                name: "system-banner");

            migrationBuilder.DropTable(
                name: "ugc-market-item");
        }
    }
}
