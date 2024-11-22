using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maple2.Server.World.Migrations {
    /// <inheritdoc />
    public partial class RemoveMailReceiverIdFK : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_mail_character_ReceiverId",
                table: "mail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddForeignKey(
                name: "FK_mail_character_ReceiverId",
                table: "mail",
                column: "ReceiverId",
                principalTable: "character",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
