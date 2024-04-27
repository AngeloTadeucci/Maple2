using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class UgcMarket : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "ugcresource",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<int>(
                name: "TabId",
                table: "ugc-market-item",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Giftable",
                table: "premium-market-item",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "GatheringCounts",
                table: "character-config",
                type: "json",
                nullable: true,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "json")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FavoriteDesigners",
                table: "character-config",
                type: "json",
                defaultValue: "{}",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ugc-market-item-sold",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Profit = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoldTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ugc-market-item-sold", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ugc-market-item-sold_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ugc-market-item-sold_AccountId",
                table: "ugc-market-item-sold",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "ugc-market-item-sold");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ugcresource");

            migrationBuilder.DropColumn(
                name: "TabId",
                table: "ugc-market-item");

            migrationBuilder.DropColumn(
                name: "Giftable",
                table: "premium-market-item");

            migrationBuilder.DropColumn(
                name: "FavoriteDesigners",
                table: "character-config");

            migrationBuilder.UpdateData(
                table: "character-config",
                keyColumn: "GatheringCounts",
                keyValue: null,
                column: "GatheringCounts",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "GatheringCounts",
                table: "character-config",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
