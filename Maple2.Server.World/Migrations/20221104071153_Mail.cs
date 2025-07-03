using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    public partial class Mail : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "mail",
                columns: table => new {
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SenderId = table.Column<long>(type: "bigint", nullable: false),
                    SenderName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TitleArgs = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentArgs = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReadTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SendTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Mail", x => new { x.ReceiverId, x.Id });
                    table.ForeignKey(
                        name: "FK_Mail_Character_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Id cannot exceed int.MaxValue (2147483647)
            migrationBuilder.Sql("ALTER TABLE `mail` AUTO_INCREMENT = 100000");

            /* https://github.com/dotnet/efcore/issues/10443
            CREATE OR REPLACE TRIGGER SetMailId BEFORE INSERT ON mail
            FOR EACH ROW BEGIN
                IF IFNULL(NEW.Id, 0) = 0 THEN
                    SET NEW.Id = (SELECT MAX(Id) FROM mail WHERE ReceiverId = NEW.ReceiverId) + 1;
                END IF;
            END
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "mail");

            // DROP TRIGGER IF EXISTS SetMailId
        }
    }
}
