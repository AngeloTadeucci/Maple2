using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class Shop : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "shop",
                columns: table => new {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Skin = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    HideUnuseable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HideStats = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisableBuyback = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OpenWallet = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisplayNew = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RandomizeOrder = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RestockTime = table.Column<long>(type: "bigint", nullable: false),
                    RestockInterval = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RestockCurrencyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ExcessRestockCurrencyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RestockCost = table.Column<int>(type: "int", nullable: false),
                    EnableRestockCostMultiplier = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TotalRestockCount = table.Column<int>(type: "int", nullable: false),
                    DisableInstantRestock = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PersistantInventory = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
                    ShopId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CurrencyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CurrencyItemId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    SalePrice = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    StockCount = table.Column<int>(type: "int", nullable: false),
                    StockPurchased = table.Column<int>(type: "int", nullable: false),
                    RequireAchievementId = table.Column<int>(type: "int", nullable: false),
                    RequireAchievementRank = table.Column<int>(type: "int", nullable: false),
                    RequireChampionshipGrade = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RequireChampionshipJoinCount = table.Column<short>(type: "smallint", nullable: false),
                    RequireGuildMerchantType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RequireGuildMerchantLevel = table.Column<short>(type: "smallint", nullable: false),
                    Quantity = table.Column<short>(type: "smallint", nullable: false),
                    Label = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CurrencyIdString = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequireQuestAllianceId = table.Column<short>(type: "smallint", nullable: false),
                    RequireFameGrade = table.Column<int>(type: "int", nullable: false),
                    AutoPreviewEquip = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
                name: "IX_shop-item_ShopId",
                table: "shop-item",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_game-event-user-value_Character_CharacterId",
                table: "game-event-user-value",
                column: "CharacterId",
                principalTable: "Character",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_game-event-user-value_Character_CharacterId",
                table: "game-event-user-value");

            migrationBuilder.DropTable(
                name: "shop-item");

            migrationBuilder.DropTable(
                name: "shop");
        }
    }
}
