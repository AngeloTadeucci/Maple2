using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class Club : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<int>(
                name: "BuffId",
                table: "club",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NameChangeCooldown",
                table: "club",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<byte>(
                name: "State",
                table: "club",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte) 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "BuffId",
                table: "club");

            migrationBuilder.DropColumn(
                name: "NameChangeCooldown",
                table: "club");

            migrationBuilder.DropColumn(
                name: "State",
                table: "club");
        }
    }
}
