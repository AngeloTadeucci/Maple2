using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations
{
    /// <inheritdoc />
    public partial class GuideRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeathPenalty",
                table: "character-config",
                newName: "GuideRecords");

            migrationBuilder.AddColumn<int>(
                name: "DeathCount",
                table: "character-config",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "DeathTick",
                table: "character-config",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeathCount",
                table: "character-config");

            migrationBuilder.DropColumn(
                name: "DeathTick",
                table: "character-config");

            migrationBuilder.RenameColumn(
                name: "GuideRecords",
                table: "character-config",
                newName: "DeathPenalty");
        }
    }
}
