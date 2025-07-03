using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class BeautyShop : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<short>(
                name: "HairSlotExpand",
                table: "character-unlock",
                type: "smallint",
                nullable: false,
                defaultValue: (short) 0);

            migrationBuilder.CreateTable(
                name: "beauty-shop",
                columns: table => new {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Unknown1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Unknown2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Category = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ShopType = table.Column<int>(type: "int", nullable: false),
                    ShopSubType = table.Column<int>(type: "int", nullable: false),
                    VoucherId = table.Column<int>(type: "int", nullable: false),
                    ServiceRewardItemId = table.Column<int>(type: "int", nullable: false),
                    ServiceCostCurrencyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ServiceCostItemId = table.Column<int>(type: "int", nullable: false),
                    ServiceCostAmount = table.Column<int>(type: "int", nullable: false),
                    RecolorCostCurrencyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RecolorCostItemId = table.Column<int>(type: "int", nullable: false),
                    RecolorCostAmount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_beauty-shop", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "beauty-shop-entry",
                columns: table => new {
                    ShopId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RequireLevel = table.Column<short>(type: "smallint", nullable: false),
                    RequireAchievementId = table.Column<int>(type: "int", nullable: false),
                    RequireAchievementRank = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CostType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CostItemId = table.Column<int>(type: "int", nullable: false),
                    CostAmount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_beauty-shop-entry", x => new { x.ShopId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_beauty-shop-entry_beauty-shop_ShopId",
                        column: x => x.ShopId,
                        principalTable: "beauty-shop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "beauty-shop-entry");

            migrationBuilder.DropTable(
                name: "beauty-shop");

            migrationBuilder.DropColumn(
                name: "HairSlotExpand",
                table: "character-unlock");
        }
    }
}
