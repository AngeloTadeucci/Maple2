using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class RemoveShops : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_character-shop-data_shop_ShopId",
                table: "character-shop-data");

            migrationBuilder.DropForeignKey(
                name: "FK_character-shop-item-data_shop-item_ShopItemId",
                table: "character-shop-item-data");

            migrationBuilder.DropForeignKey(
                name: "FK_character-shop-item-data_shop_ShopId",
                table: "character-shop-item-data");

            migrationBuilder.DropTable(
                name: "shop-item");

            migrationBuilder.DropTable(
                name: "shop");

            migrationBuilder.DropPrimaryKey(
                name: "PK_character-shop-item-data",
                table: "character-shop-item-data");

            migrationBuilder.DropIndex(
                name: "IX_character-shop-item-data_ShopId",
                table: "character-shop-item-data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_character-shop-item-data",
                table: "character-shop-item-data",
                columns: new[] { "ShopItemId", "ShopId", "OwnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropPrimaryKey(
                name: "PK_character-shop-item-data",
                table: "character-shop-item-data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_character-shop-item-data",
                table: "character-shop-item-data",
                columns: new[] { "ShopItemId", "OwnerId" });

            migrationBuilder.CreateTable(
                name: "shop",
                columns: table => new {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DisableBuyback = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisplayNew = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HideStats = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HideUnuseable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OpenWallet = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RandomizeOrder = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RestockData = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RestockTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Skin = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_shop", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop-item",
                columns: table => new {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AutoPreviewEquip = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyItemId = table.Column<int>(type: "int", nullable: false),
                    CurrencyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    IconCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<short>(type: "smallint", nullable: false),
                    Rarity = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RequireAchievementId = table.Column<int>(type: "int", nullable: false),
                    RequireAchievementRank = table.Column<int>(type: "int", nullable: false),
                    RequireChampionshipGrade = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RequireChampionshipJoinCount = table.Column<short>(type: "smallint", nullable: false),
                    RequireFameGrade = table.Column<int>(type: "int", nullable: false),
                    RequireGuildMerchantLevel = table.Column<short>(type: "smallint", nullable: false),
                    RequireGuildMerchantType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RequireGuildTrophy = table.Column<int>(type: "int", nullable: false),
                    RequireQuestAllianceId = table.Column<short>(type: "smallint", nullable: false),
                    RestrictedBuyData = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SalePrice = table.Column<int>(type: "int", nullable: false),
                    ShopId = table.Column<int>(type: "int", nullable: false),
                    StockCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_shop-item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shop-item_shop_ShopId",
                        column: x => x.ShopId,
                        principalTable: "shop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_character-shop-item-data_ShopId",
                table: "character-shop-item-data",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_shop-item_ShopId",
                table: "shop-item",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_character-shop-data_shop_ShopId",
                table: "character-shop-data",
                column: "ShopId",
                principalTable: "shop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_character-shop-item-data_shop-item_ShopItemId",
                table: "character-shop-item-data",
                column: "ShopItemId",
                principalTable: "shop-item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_character-shop-item-data_shop_ShopId",
                table: "character-shop-item-data",
                column: "ShopId",
                principalTable: "shop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
