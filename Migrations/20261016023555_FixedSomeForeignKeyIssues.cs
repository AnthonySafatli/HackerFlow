using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HackerFlow.Migrations
{
    /// <inheritdoc />
    public partial class FixedSomeForeignKeyIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Companies_CompanyId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Connections_ConnectionId",
                table: "Applications");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Companies_CompanyId",
                table: "Applications",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Connections_ConnectionId",
                table: "Applications",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Companies_CompanyId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Connections_ConnectionId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Name",
                table: "Companies");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Companies_CompanyId",
                table: "Applications",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Connections_ConnectionId",
                table: "Applications",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "Id");
        }
    }
}
