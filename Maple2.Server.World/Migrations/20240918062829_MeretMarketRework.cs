using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations
{
    /// <inheritdoc />
    public partial class MeretMarketRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "premium-market-item");

            migrationBuilder.CreateTable(
                name: "meret-market-sold",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MarketId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    SoldTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meret-market-sold", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meret-market-sold");

            migrationBuilder.CreateTable(
                name: "premium-market-item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BannerLabel = table.Column<int>(type: "int", nullable: false),
                    BannerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BonusQuantity = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CurrencyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Giftable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ItemDuration = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    JobRequirement = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    PcCafe = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    PromoData = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RequireAchievementId = table.Column<int>(type: "int", nullable: false),
                    RequireAchievementRank = table.Column<int>(type: "int", nullable: false),
                    RequireMaxLevel = table.Column<short>(type: "smallint", nullable: false),
                    RequireMinLevel = table.Column<short>(type: "smallint", nullable: false),
                    RestockUnavailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SalePrice = table.Column<long>(type: "bigint", nullable: false),
                    SalesCount = table.Column<int>(type: "int", nullable: false),
                    SellBeginTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SellEndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ShowSaleTime = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TabId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_premium-market-item", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
