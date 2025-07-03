using Maple2.Server.Core.Constants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class InteractCubeFix : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            var dbName = Environment.GetEnvironmentVariable("GAME_DB_NAME");
            migrationBuilder.Sql($"DELETE FROM `{dbName}`.`ugcmap-cube` WHERE `Interact` IS NOT NULL AND `Interact` <> '';");
            migrationBuilder.Sql($"DELETE FROM `{dbName}`.`home-layout-cube` WHERE `Interact` IS NOT NULL AND `Interact` <> '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {

        }
    }
}
