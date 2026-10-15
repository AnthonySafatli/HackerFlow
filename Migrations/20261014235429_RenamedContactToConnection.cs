using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HackerFlow.Migrations
{
    /// <inheritdoc />
    public partial class RenamedContactToConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Connections_ContactId",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "Applications",
                newName: "ConnectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Applications_ContactId",
                table: "Applications",
                newName: "IX_Applications_ConnectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Connections_ConnectionId",
                table: "Applications",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Connections_ConnectionId",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "ConnectionId",
                table: "Applications",
                newName: "ContactId");

            migrationBuilder.RenameIndex(
                name: "IX_Applications_ConnectionId",
                table: "Applications",
                newName: "IX_Applications_ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Connections_ContactId",
                table: "Applications",
                column: "ContactId",
                principalTable: "Connections",
                principalColumn: "Id");
        }
    }
}
