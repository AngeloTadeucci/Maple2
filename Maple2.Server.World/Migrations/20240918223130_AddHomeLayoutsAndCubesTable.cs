using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class AddHomeLayoutsAndCubesTable : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "Blueprints",
                table: "home",
                type: "json",
                defaultValue: "[]",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Layouts",
                table: "home",
                type: "json",
                defaultValue: "[]",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "home-layout",
                columns: table => new {
                    Uid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Area = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Height = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_home-layout", x => x.Uid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "home-layout-cube",
                columns: table => new {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HomeLayoutId = table.Column<long>(type: "bigint", nullable: false),
                    X = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Y = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Z = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Rotation = table.Column<float>(type: "float", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Template = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table => {
                    table.PrimaryKey("PK_home-layout-cube", x => x.Id);
                    table.ForeignKey(
                        name: "FK_home-layout-cube_home-layout_HomeLayoutId",
                        column: x => x.HomeLayoutId,
                        principalTable: "home-layout",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_home-layout-cube_HomeLayoutId",
                table: "home-layout-cube",
                column: "HomeLayoutId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "home-layout-cube");

            migrationBuilder.DropTable(
                name: "home-layout");

            migrationBuilder.DropColumn(
                name: "Blueprints",
                table: "home");

            migrationBuilder.DropColumn(
                name: "Layouts",
                table: "home");
        }
    }
}
