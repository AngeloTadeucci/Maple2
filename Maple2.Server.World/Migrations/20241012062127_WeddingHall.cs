using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations
{
    /// <inheritdoc />
    public partial class WeddingHall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WeddingInvite",
                table: "mail",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "wedding-hall",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MarriageId = table.Column<long>(type: "bigint", nullable: false),
                    CeremonyTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    PackageHallId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    Public = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GuestList = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wedding-hall", x => x.Id);
                    table.ForeignKey(
                        name: "FK_wedding-hall_marriage_MarriageId",
                        column: x => x.MarriageId,
                        principalTable: "marriage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_wedding-hall_MarriageId",
                table: "wedding-hall",
                column: "MarriageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wedding-hall");

            migrationBuilder.DropColumn(
                name: "WeddingInvite",
                table: "mail");
        }
    }
}
