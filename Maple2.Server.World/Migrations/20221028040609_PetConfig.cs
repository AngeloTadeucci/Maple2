using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    public partial class PetConfig : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "pet-config",
                columns: table => new {
                    ItemUid = table.Column<long>(type: "bigint", nullable: false),
                    PotionConfigs = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LootConfig = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table => {
                    table.PrimaryKey("PK_pet-config", x => x.ItemUid);
                    table.ForeignKey(
                        name: "FK_pet-config_Item_ItemUid",
                        column: x => x.ItemUid,
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "pet-config");
        }
    }
}
