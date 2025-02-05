using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    public partial class ItemJson : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            // Type must appear first in the JSON to properly deserialize: https://github.com/dotnet/runtime/issues/72604
            // Using "!" as the key because MySQL seems to sort and so this will always be first.
            migrationBuilder.Sql(@"UPDATE `item` SET
                Appearance = REPLACE(Appearance, '""Type"": 1', '""!"": ""default""'),
                Appearance = REPLACE(Appearance, '""Type"": 2', '""!"": ""hair""'),
                Appearance = REPLACE(Appearance, '""Type"": 3', '""!"": ""decal""'),
                Appearance = REPLACE(Appearance, '""Type"": 4', '""!"": ""cap""')");
            migrationBuilder.Sql(@"UPDATE `item` SET
                SubType = REPLACE(SubType, '""Type"": 1', '""!"": ""ugc""'),
                SubType = REPLACE(SubType, '""Type"": 2', '""!"": ""pet""'),
                SubType = REPLACE(SubType, '""Type"": 3', '""!"": ""music""'),
                SubType = REPLACE(SubType, '""Type"": 4', '""!"": ""badge""')");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql(@"UPDATE `item` SET
                Appearance = REPLACE(Appearance, '""!"": ""default""', '""Type"": 1'),
                Appearance = REPLACE(Appearance, '""!"": ""hair""', '""Type"": 2'),
                Appearance = REPLACE(Appearance, '""!"": ""decal""', '""Type"": 3'),
                Appearance = REPLACE(Appearance, '""!"": ""cap""', '""Type"": 4')");
            migrationBuilder.Sql(@"UPDATE `item` SET
                SubType = REPLACE(SubType, '""!"": ""ugc""', '""Type"": 1'),
                SubType = REPLACE(SubType, '""!"": ""pet""', '""Type"": 2'),
                SubType = REPLACE(SubType, '""!"": ""music""', '""Type"": 3'),
                SubType = REPLACE(SubType, '""!"": ""badge""', '""Type"": 4')");
        }
    }
}
