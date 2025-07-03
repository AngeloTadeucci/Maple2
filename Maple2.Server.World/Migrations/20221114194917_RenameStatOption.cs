using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    public partial class RenameStatOption : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql(@"UPDATE `item` SET
                Stats = REPLACE(Stats, '""StatOption"":', '""BasicOption"":')");
            migrationBuilder.Sql(@"UPDATE `item` SET
                Enchant = REPLACE(Enchant, '""StatOption"":', '""BasicOption"":')");
            migrationBuilder.Sql(@"UPDATE `item` SET
                LimitBreak = REPLACE(LimitBreak, '""StatOption"":', '""BasicOption"":')");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql(@"UPDATE `item` SET
                Stats = REPLACE(Stats, '""BasicOption"":', '""StatOption"":')");
            migrationBuilder.Sql(@"UPDATE `item` SET
                Enchant = REPLACE(Enchant, '""BasicOption"":', '""StatOption"":')");
            migrationBuilder.Sql(@"UPDATE `item` SET
                LimitBreak = REPLACE(LimitBreak, '""BasicOption"":', '""StatOption"":')");
        }
    }
}
