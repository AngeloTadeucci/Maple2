using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    public partial class ChatSticker : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "Stamps",
                table: "character-unlock",
                newName: "StickerSets");

            migrationBuilder.AddColumn<string>(
                name: "FavoriteStickers",
                table: "character-config",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"UPDATE `character-unlock` SET StickerSets = REPLACE(StickerSets, '[]', '{}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "FavoriteStickers",
                table: "character-config");

            migrationBuilder.RenameColumn(
                name: "StickerSets",
                table: "character-unlock",
                newName: "Stamps");

            migrationBuilder.Sql(@"UPDATE `character-unlock` SET Stamps = REPLACE(Stamps, '{}', '[]')");
        }
    }
}
